using ClosedXML.Excel;
using Domain.Classes;
using DB;


namespace TableReader;

public class TrainExtractor : ITableReader
{
    private readonly AppDbContext ldb;
    public TrainExtractor(AppDbContext db)
    {
        ldb = db;
    }
    
    
    public Task ExtractFromFile(FileStream fs, int year, int month)
    {
        Transaction note = new Transaction();
        note.Year = year;
        note.Month = month;
        
        
        using var book = new XLWorkbook(fs);
        
        List<Station> existStations = ldb.Stations.ToList();
        
        if (book.Worksheets.Count != 3) throw new Exception("Wrong number of sheets");
        
        int[] FirstRows = new int[3];
        var TrainDataList = book.Worksheet(1);
        FirstRows[0] = GetFirstRowIndex(TrainDataList);
        var PassengerDataList = book.Worksheet(2);
        FirstRows[1] = GetFirstRowIndex(PassengerDataList);
        var PaymentDataList = book.Worksheet(3);
        FirstRows[2] = GetFirstRowIndex(PaymentDataList);

        
        List<Station> stations = new List<Station>();
        Train[] trains = new Train[GetTrainCount(TrainDataList, FirstRows[0])];
        for (int i = 0; i < trains.Length; i++) trains[i] = new Train();
        int counter = 0;
        int refCounter = 0;
        //Достаем Поезда
        foreach (Train train in trains)
        {
            train.Transaction = note;
            
            train.Period = $"{year}{month}";
            train.Number = TrainDataList.Cell(FirstRows[0]+counter+refCounter, 2).Value.ToString();
            
            //Станции
            //!!!!Добавить поиск Станции в БД и запрашивать её у пользователя, если таковой не нашлось
            string[] Stations = TrainDataList.Cell(FirstRows[0] + counter + refCounter, 3).Value.ToString().Split(new char[]{'–','-'});
            if (Stations.Length == 2)
            {
                train.StationFrom = GetStationOrNew(Stations[0]);
                train.StationTo = GetStationOrNew(Stations[1]);
            }
            if (Stations.Length == 3)
            {
                train.StationFrom = GetStationOrNew(Stations[0]);
                train.StationMiddle = GetStationOrNew(Stations[1]);
                train.StationTo = GetStationOrNew(Stations[2]);
            }
            
            //Время
            string[] Time = TrainDataList.Cell(FirstRows[0] + counter + refCounter, 4).Value.ToString().Replace(".",":").Split(new char[]{'–','-'});
            train.TimeFrom = TimeOnly.Parse(Time[0]);
            train.TimeTo = TimeOnly.Parse(Time[1]);
            
            //Метрики
            train.Distance = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 5).Value.ToString());
            train.RailcarCount = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 6).Value.ToString());
            train.RangePerDay = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 7).Value.ToString());
            train.DayInRaise = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 8).Value.ToString());
            train.RangePerMonth = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter + refCounter, 9).Value.ToString());

           
            
            
            Console.WriteLine($"{train.Number} | \t{counter}/{trains.Length}");
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
            
            route.Casual = GetCategoryData(counter, 0);
            route.Student = GetCategoryData(counter, 1);
            route.FedBenefit = GetCategoryData(counter, 2);
            route.RegBenefit = GetCategoryData(counter, 3);
            route.Another = GetCategoryData(counter, 4);
            Console.WriteLine($"{route.RouteId} {route.Casual.Count}");
            counter++;
        }
        WriteToBase();
        return null;

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
        
        PasCategory GetCategoryData(int row, int categoryNumber)
        {
            return new PasCategory()
            {
                Count = Convert.ToInt32(GetValueOrZero(PassengerDataList.Cell(FirstRows[1] + counter, 3 + categoryNumber))),
                WayLength = Convert.ToDouble(GetValueOrZero(PassengerDataList.Cell(FirstRows[1] + counter, 8 + categoryNumber))),
                Payment = Convert.ToDouble(GetValueOrZero(PaymentDataList.Cell(FirstRows[2] + counter, 3 + categoryNumber))),
                PaymentBySubject = Convert.ToDouble(GetValueOrZero(PaymentDataList.Cell(FirstRows[2] + counter, 8 + categoryNumber)))
            };
        }

        void WriteToBase()
        {
            note.UnitsGet = stations.Count + trains.Length + routes.Length;
            ldb.Stations.AddRange(stations);
            ldb.Routes.AddRange(routes);
            ldb.SaveChanges();
        }

    }
    
    private string GetValueOrZero(IXLCell cell)
    {
        string data = cell.Value.ToString().Replace("*", "").Trim();
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

    private int GetTrainCount(IXLWorksheet worksheet, int TableStart)
    {
        int counter = TableStart;
        string value = "1";
        string id = "1";
        while (id != "" || value != "")
        {
            counter++;
            id = worksheet.Cell(counter, 1).Value.ToString();
            value = worksheet.Cell(counter, 2).Value.ToString().Split('/')[0];
        }
        return counter-1-TableStart;
    }
}

