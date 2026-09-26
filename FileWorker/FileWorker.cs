using System.Globalization;
using System.Security.Claims;
using ClosedXML.Excel;
using Domain.Classes;
using DB;
using DB.Repositories;
using Domain.DTO;
using Microsoft.Extensions.DependencyInjection;


namespace FileWorker;

public class FileWorkerService : IFileWorker
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IStationRepo _stationRepo;
    private readonly IRouteRepo _routeRepo;
    private readonly IUserRepo _userRepo;
    public FileWorkerService
        ( ITransactionRepo transactionRepo, IStationRepo stationRepo, IRouteRepo routeRepo, IUserRepo userRepo)
    {
        _stationRepo = stationRepo;
        _transactionRepo = transactionRepo;
        _routeRepo = routeRepo;
        _userRepo = userRepo;
    }
    
    
    public async Task<List<TrainDto>> ExtractFromFile(FileStream fs, UploadFileDto uploadDto, ClaimsPrincipal user)
    {
        if (user.Identity is null || user.Identity.Name is null) throw new Exception("Отказано в доступе");
        
        User? dbUser = await _userRepo.GetUserByUsernameAsync(user.Identity.Name);
        
        if (dbUser is null) throw new Exception("Пользователь с таким именем не найден");

        int year = uploadDto.year;
        int month = uploadDto.month;
        
        Transaction? tr = await _transactionRepo.GetTransactionByYearAndMonthAsync(year, month);
        if (tr is not null && tr.Type == Transaction.TransactionType.AddFile)
        {
            throw new Exception(
                "В базе данных уже есть записи датированные данным периодом, во избежание конфликта запись отклонена.");
        }
        
        
        Transaction note = new Transaction();
        note.Year = year;
        note.Month = month;
        note.User = dbUser;
        note.UserId = dbUser.Id;
        note.Description = uploadDto.description is null ? "" : uploadDto.description;
        note.Type = Transaction.TransactionType.AddFile;
        
        
        using var book = new XLWorkbook(fs);
        
        
        if (book.Worksheets.Count != 3) throw new Exception("Wrong number of sheets");
        
        int[][] Columns = new int[2][]; //Номера столбцов подлежащих записи (нужно только для запси routes)
        
        int[] FirstRows = new int[3]; // Первые строки каждого из 3 листов
        
        var TrainDataList = book.Worksheet(1);
        FirstRows[0] = GetFirstRowIndex(TrainDataList);
        
        var PassengerDataList = book.Worksheet(2);
        FirstRows[1] = GetFirstRowIndex(PassengerDataList);
        Columns[0] = GetNumeredColumns(FirstRows[1], PassengerDataList);
        
        var PaymentDataList = book.Worksheet(3);
        FirstRows[2] = GetFirstRowIndex(PaymentDataList);
        Columns[1] = GetNumeredColumns(FirstRows[2], PaymentDataList);
        
         List<Station> existStations = await _stationRepo.GetAllStationsAsync();
        
        List<Station> stations = new List<Station>();
        List<Train> trainWithDesc = new List<Train>();
        Train[] trains = new Train[GetTrainCount(TrainDataList, FirstRows[0])];
        for (int i = 0; i < trains.Length; i++) trains[i] = new Train();
        int counter = 0;
        int refCounter = 0;
        //Достаем Поезда
        foreach (Train train in trains)
        {
            train.Transaction = note;
            
            train.year = year;
            train.month = month;
            train.Number = TrainDataList.Cell(FirstRows[0]+counter+refCounter, 2).Value.ToString();
            if (train.Number.Contains("*"))
            {
                train.HasDesc =  true;
                train.Number = train.Number.Replace("*", "");
                train.Description = $"Описание для {train.Number}";
            }
            else
            {
                train.HasDesc = false;
            }
            
            //Станции
            string[] Stations = TrainDataList.Cell(FirstRows[0] + counter + refCounter, 3).Value.ToString().Split(new char[]{'–','-'});
            if (Stations.Length == 2)
            {
                train.StationFrom = GetStationOrNew(Stations[0]);
                train.StationTo = GetStationOrNew(Stations[1]);
            }
            else if (Stations.Length == 3)
            {
                train.StationFrom = GetStationOrNew(Stations[0]);
                train.StationMiddle = GetStationOrNew(Stations[1]);
                train.StationTo = GetStationOrNew(Stations[2]);
            }
            else if (Stations.Length > 3 ) throw  new Exception($"Не получилось получить станции в строке {counter + FirstRows[0] + refCounter}. Проверьте количество тире в названии станции.");
            
            //Время
            string[] Time = TrainDataList.Cell(FirstRows[0] + counter + refCounter, 4).Value.ToString().Replace(".",":").Split(new char[]{'–','-','-'});
            train.TimeFrom = TimeOnly.Parse(Time[0]);
            train.TimeTo = TimeOnly.Parse(Time[1]);
            
            //Метрики
            train.Distance = GetClearInt(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 5).Value.ToString());
            train.RailcarCount = GetClearInt(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 6).Value.ToString());
            train.RangePerDay = GetClearInt(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 7).Value.ToString());
            train.DayInRaise = GetClearInt(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 8).Value.ToString());
            train.RangePerMonth = GetClearInt(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 9).Value.ToString());

            train.RowInFile = counter + FirstRows[0];
            
            if (train.Distance == 0 || train.RangePerDay == 0) train.IsCanceled = true;
            if (train.HasDesc) trainWithDesc.Add(train);
            
            
            
            int GetClearInt(string value)
            {
                train.HasDesc = value.Contains("*") || train.HasDesc;
                value = value.Trim().Replace("*", "");
                return Convert.ToInt32(value);
            }
            
            counter++;
        }
        //Достаем маршруты
        Route[] routes = new Route[GetTrainCount(PassengerDataList, FirstRows[1])];
        for (int i = 0; i < routes.Length; i++) routes[i] = new Route();
        counter = 0;
        foreach (Route route in routes)
        {
            route.Transaction = note;
            
            route.Year = year;
            route.Month = month;
            
            string number = PassengerDataList.Cell(FirstRows[1] + counter, 2).Value.ToString().Trim();
            route.RouteNumber = number.ToLower().Contains("ручную") ? "Ручной ввод" : number;

            var RouteTrains = trains.Where(t => t.Number.Contains(route.RouteNumber)).ToList();
            route.Trains = RouteTrains;
            foreach (var train in RouteTrains) train.Route = route;
            
            route.Casual = GetCategoryData(0);
            route.Student = GetCategoryData(1);
            route.FedBenefit = GetCategoryData(2);
            route.RegBenefit = GetCategoryData(3);
            route.Another = GetCategoryData(4);
            
            route.RowInFile =  counter + FirstRows[1];
            counter++;
        }
        await WriteToBase();

        var dtoWithNoDesc = writeDto();
        
        return dtoWithNoDesc;

        Station GetStationOrNew(string stationName)
        {
            stationName = stationName.Trim();
            var s = existStations.FirstOrDefault(s => s.Name == stationName);
            s = s == null ? stations.FirstOrDefault(s => s.Name == stationName) : s;
            if (s == null)
            {
                Station station = new Station()
                {
                    Name = stationName,
                    Transaction = note
                }; 
                stations.Add(station);
                return station;
            }
            else return s;
        }
        
        PasCategory GetCategoryData(int categoryNumber)
        {
            var Pcat = new PasCategory();
            
            Console.WriteLine($"{FirstRows[1]+counter} | {Columns[0][3 + categoryNumber]}");
            Pcat.Count = Convert.ToInt32(
                GetValueOrZero(PassengerDataList.Cell(FirstRows[1] + counter, Columns[0][3 + categoryNumber])));
            Pcat.WayLength = double.Parse(
                GetValueOrZero(PassengerDataList.Cell(FirstRows[1] + counter, Columns[0][8 + categoryNumber])),
                CultureInfo.InvariantCulture);
            Pcat.Payment = double.Parse(
                GetValueOrZero(PaymentDataList.Cell(FirstRows[2] + counter, Columns[1][3 + categoryNumber])),
                CultureInfo.InvariantCulture);
            
            
            if (categoryNumber != 0)
                Pcat.PaymentBySubject =
                    double.Parse(GetValueOrZero(PaymentDataList.Cell(FirstRows[2] + counter,
                        Columns[1][7 + categoryNumber])), CultureInfo.InvariantCulture);
            else Pcat.PaymentBySubject = 0;
            return Pcat;
        }

        async Task<string> WriteToBase()
        {
            note.UnitsGet = stations.Count + trains.Length + routes.Length;
            await _transactionRepo.WriteNewTransactionAsync(note);
            await _stationRepo.WriteStationsAsync(stations);
            await _routeRepo.WriteRoutesAsync(routes);
            return "ok";
        }

        List<TrainDto> writeDto()
        {
            List<TrainDto> dtos = new List<TrainDto>();
            foreach (var train in trainWithDesc)
            {
                dtos.Add(new TrainDto(train));
            }
            return dtos;
        }
        
        string GetValueOrZero(IXLCell cell)
        {
            string data = cell.Value
                .ToString()
                .Replace("*", "")
                .Replace(",", ".")
                .Trim();
            return data == "" ?  "0" : data;
        }

        int GetFirstRowIndex(IXLWorksheet worksheet)
        {
            int counter = 0;
            string IdValue = "";
            string NextValue = "";
            while (IdValue != "1" || NextValue == IdValue )
            {
                counter++;
                IdValue = worksheet.Cell(counter, 1).Value.ToString();
                NextValue = worksheet.Cell(counter+1, 1).Value.ToString();
            }
            return counter;
        }

        int[] GetNumeredColumns(int firstRow, IXLWorksheet worksheet)
        {
            int row = firstRow - 1; //Переходим на строку с нумерацией
            List<int> columns = new List<int>();
            columns.Add(0);
            int colCounter = 1;
            int referenceNumber = 1;
            int nullCells = 0;
            while (columns.Count <= 12)
            {
                if (worksheet.Cell(row, colCounter).Value.ToString() == referenceNumber.ToString())
                {
                    nullCells = 0;
                    referenceNumber++;
                    columns.Add(colCounter);
                    colCounter++;
                }
                else
                {
                    colCounter++;
                    nullCells++;
                    if (nullCells > 2) break;
                }
            }
            return columns.ToArray();

        }

        int GetTrainCount(IXLWorksheet worksheet, int TableStart)
        {
            int counter = TableStart;
            string value = "1";
            string id = "1";
            while (id != "" || (value != "" && int.TryParse(value, out _)))
            {
                counter++;
                id = worksheet.Cell(counter, 1).Value.ToString();
                value = worksheet.Cell(counter, 2).Value.ToString().Split('/')[0].Replace("*", "");
            }
            return counter-1-TableStart;
        }
        
    }

    public async Task<string?> CreateFile(RouteFilterDto filter, MemoryStream buffer)
    {
        List<Route> routes;
        string fileName = $"{DateTime.Now}.xlsx";
        routes = await _routeRepo.GetRoutesByFilterAsync(filter, true);
        if (routes.Count == 0) throw new Exception("По заданным фильтрам маршруты не обнаружены");
        var wBook = CreateNewBook();
        var trainSheet = wBook.Worksheet("Поезда");
        var passSheet = wBook.Worksheet("Пассажиропоток");
        var paySheet = wBook.Worksheet("Доходы");

        int tRow = 2;
        int rRow = 3;
        
        for (int i = 0; i < routes.Count; i++)
        {
            for (int j = 0; j < routes[i].Trains.Count; j++)
            {
                tRow++;
                WriteTrain(routes[i].Trains[j], tRow);
            }
            rRow++;
            WriteRoutePass(routes[i], rRow);
            WriteRoutePaym(routes[i], rRow);
        }
        wBook.SaveAs(buffer);
        return fileName;


        void WriteTrain(Train train, int row)
        {
            trainSheet.Cell(row, "A").Value = row - 2;
            trainSheet.Cell(row, "B").Value = train.month + "." + train.year;
            trainSheet.Cell(row, "C").Value = train.HasDesc ? train.Number + "*": train.Number;

            var stations = new List<string>();
            stations.Add(train.StationFrom.Name);
            if (train.StationMiddleId is not null) stations.Add(train.StationMiddle.Name);
            stations.Add(train.StationTo.Name);
            trainSheet.Cell(row, "D").Value = String.Join("-", stations);
            
            trainSheet.Cell(row, "E").Value = 
                train.TimeFrom.ToString("HH:mm") + "-" + 
                train.TimeTo.ToString("HH:mm") ;
            trainSheet.Cell(row, "F").Value = train.Distance;
            trainSheet.Cell(row, "G").Value = train.RailcarCount;
            trainSheet.Cell(row, "H").Value = train.RangePerDay;
            trainSheet.Cell(row, "I").Value = train.DayInRaise;
            trainSheet.Cell(row, "J").Value = train.RangePerMonth;
            trainSheet.Cell(row, "K").Value = train.Description;
        }

        void WriteRoutePass(Route route, int row)
        {
            passSheet.Cell(row, "A").Value = row - 3;
            passSheet.Cell(row, "B").Value = route.Month + "." + route.Year;
            passSheet.Cell(row, "C").Value = route.RouteNumber;
            
            passSheet.Cell(row, "D").Value = route.Casual.Count;
            passSheet.Cell(row, "E").Value = route.Student.Count;
            passSheet.Cell(row, "F").Value = route.FedBenefit.Count;
            passSheet.Cell(row, "G").Value = route.RegBenefit.Count;
            passSheet.Cell(row, "H").Value = route.Another.Count;
            
            passSheet.Cell(row, "I").Value = route.Casual.WayLength;
            passSheet.Cell(row, "J").Value = route.Student.WayLength;
            passSheet.Cell(row, "K").Value = route.FedBenefit.WayLength;
            passSheet.Cell(row, "L").Value = route.RegBenefit.WayLength;
            passSheet.Cell(row, "M").Value = route.Another.WayLength;
        }

        void WriteRoutePaym(Route route, int row)
        {
            paySheet.Cell(row, "A").Value = row - 3;
            paySheet.Cell(row, "B").Value = route.Month + "." + route.Year;
            paySheet.Cell(row, "C").Value = route.RouteNumber;
            paySheet.Cell(row, "D").Value = route.Casual.Payment;
            paySheet.Cell(row, "E").Value = route.Student.Payment;
            paySheet.Cell(row, "F").Value = route.FedBenefit.Payment;
            paySheet.Cell(row, "G").Value = route.RegBenefit.Payment;
            paySheet.Cell(row, "H").Value = route.Another.Payment;
            
            paySheet.Cell(row, "I").Value = route.Student.PaymentBySubject;
            paySheet.Cell(row, "J").Value = route.FedBenefit.PaymentBySubject;
            paySheet.Cell(row, "K").Value = route.RegBenefit.PaymentBySubject;
            paySheet.Cell(row, "L").Value = route.Another.PaymentBySubject;
        }
        
        
        XLWorkbook CreateNewBook()
        {
            XLWorkbook book = new();
            var trainsSheet = book.Worksheets.Add("Поезда");
            var passSheet = book.Worksheets.Add("Пассажиропоток");
            var paymSheet = book.Worksheets.Add("Доходы");


            trainsSheet.Cell("A1").Value = "1. Объем вагоно-километровой работы:";
            trainsSheet.Range("A1:K1").Merge(); 
            
            trainsSheet.Cell("A2").Value = "№ п/п";
            trainsSheet.Cell("B2").Value = "Период";
            trainsSheet.Cell("C2").Value = "№ поезда";
            trainsSheet.Cell("D2").Value = "Станция отправления – станция назначения";
            trainsSheet.Cell("E2").Value = "Время отправления и прибытия по конечным станциям";
            trainsSheet.Cell("F2").Value = "Расстояние между станциями, км";
            trainsSheet.Cell("G2").Value = "Количество вагонов, ед.";
            trainsSheet.Cell("H2").Value = "Вагоно-километры в сутки";
            trainsSheet.Cell("I2").Value = "Количество дней курсирования";
            trainsSheet.Cell("J2").Value = "Вагоно-километры в месяц";
            trainsSheet.Cell("K2").Value = "Примечание";
            
            trainsSheet.Column("A").Width = 6;
            trainsSheet.Column("B").Width = 12;
            trainsSheet.Column("C").Width = 10;
            trainsSheet.Column("D").Width = 35;
            trainsSheet.Column("E").Width = 35;
            trainsSheet.Column("F").Width = 18;
            trainsSheet.Column("G").Width = 15;
            trainsSheet.Column("H").Width = 18;
            trainsSheet.Column("I").Width = 18;
            trainsSheet.Column("J").Width = 18;
            trainsSheet.Column("K").Width = 50;
            
            var trainsHeaderRange = trainsSheet.Range("A2:K2");
            trainsHeaderRange.Style.Font.Bold = true;
            trainsHeaderRange.Style.Alignment.WrapText = true;
            trainsHeaderRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            trainsHeaderRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            trainsHeaderRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            
            passSheet.Cell("A1").Value = "2. Фактическое количество перевезенных пассажиров:";
            passSheet.Range("A1:L1").Merge();
            
            passSheet.Cell("A2").Value = "№ п/п";
            passSheet.Cell("B2").Value = "Период";
            passSheet.Cell("C2").Value = "№ поездов (по направлениям \"туда-обратно\")";
            passSheet.Cell("D2").Value = "Количество перевезенных пассажиров, чел.";
            passSheet.Cell("I2").Value = "Средняя дальность поездки, км.";

            passSheet.Range("D2:H2").Merge(); 
            passSheet.Range("I2:M2").Merge();
            
            passSheet.Range("A2:A3").Merge();
            passSheet.Range("B2:B3").Merge();
            passSheet.Range("C2:C3").Merge();
            
            string[] subHeaders = new string[]
            {
                "Пассажиры, не имеющие льгот",
                "Обучающиеся (скидка по провозной плате 50%)",
                "Федеральные льготники",
                "Региональные льготники, за исключением обучающихся",
                "Иные пассажиры, которым были предоставлены льготы (дети от 7 до 14 лет, работники пользующиеся Ф-4)"
            };
            
            for (int i = 0; i < subHeaders.Length; i++)
                passSheet.Cell(3, 4 + i).Value = subHeaders[i];
            
            for (int i = 0; i < subHeaders.Length; i++)
                passSheet.Cell(3, 9 + i).Value = subHeaders[i];
            
            passSheet.Column("A").Width = 6;
            passSheet.Column("C").Width = 22;
            for (int col = 3; col <= 13; col++)
                passSheet.Column(col).Width = 20;
            
            var passHeaderRange = passSheet.Range("A2:M3");
            passHeaderRange.Style.Font.Bold = true;
            passHeaderRange.Style.Alignment.WrapText = true;
            passHeaderRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            passHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            passHeaderRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            passHeaderRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            
            paymSheet.Cell("A1").Value = "3. Размер полученных доходов:";
            paymSheet.Range("A1:L1").Merge();
            
            paymSheet.Cell("A2").Value = "№ п/п";
            paymSheet.Cell("B2").Value = "Период";
            paymSheet.Cell("C2").Value = "№ поездов (по направлениям \"туда-обратно\")";
            paymSheet.Cell("D2").Value = "Доходы, полученные от перевозки пассажиров, руб.";
            paymSheet.Cell("I2").Value = "Доходы, причитающиеся от субъектов, установивших льготы, руб.";
            
            paymSheet.Range("D2:H2").Merge();
            paymSheet.Range("I2:L2").Merge();
            
            paymSheet.Range("A2:A3").Merge();
            paymSheet.Range("B2:B3").Merge();
            paymSheet.Range("C2:C3").Merge();
            
            for (int i = 0; i < subHeaders.Length; i++)
                paymSheet.Cell(3, 4 + i).Value = subHeaders[i];

            for (int i = 1; i < subHeaders.Length; i++)
                paymSheet.Cell(3, 8 + i).Value = subHeaders[i];
            
            paymSheet.Column("A").Width = 6;
            paymSheet.Column("B").Width = 22;
            for (int col = 3; col <= 12; col++)
                paymSheet.Column(col).Width = 20;

            var paymHeaderRange = paymSheet.Range("A2:L3");
            paymHeaderRange.Style.Font.Bold = true;
            paymHeaderRange.Style.Alignment.WrapText = true;
            paymHeaderRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            paymHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            paymHeaderRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            paymHeaderRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

            return book;
        }
    }
}

