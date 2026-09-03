using System.Globalization;
using System.Security.Claims;
using ClosedXML.Excel;
using Domain.Classes;
using DB;
using DB.Repositories;
using Domain.DTO;


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
        if (tr is not null)
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
            
            string number = PassengerDataList.Cell(FirstRows[1] + counter, 2).Value.ToString();
            route.RouteNumber = number.ToLower().Contains("ручную") ? "0" : number;

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
        
    }
    
    private string GetValueOrZero(IXLCell cell)
    {
        string data = cell.Value
            .ToString()
            .Replace("*", "")
            .Replace(",", ".")
            .Trim();
        return data == "" ?  "0" : data;
    }

    private int GetFirstRowIndex(IXLWorksheet worksheet)
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

    private int[] GetNumeredColumns(int firstRow, IXLWorksheet worksheet)
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

    private int GetTrainCount(IXLWorksheet worksheet, int TableStart)
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

