using System.Runtime.InteropServices.ComTypes;
using ClosedXML.Excel;
using DB;
using DocumentFormat.OpenXml.Spreadsheet;


namespace TableReader;

class TrainExtractor
{
    public void Extract(FileStream fs, int year, int month)
    {
        using var book = new XLWorkbook(fs);
        
        if (book.Worksheets.Count != 3) return;

        int[] FirstRows = new int[3];
        var TrainDataList = book.Worksheet(1);
        FirstRows[0] = GetFirstRowIndex(TrainDataList);
        var PassengerDataList = book.Worksheet(2);
        FirstRows[1] = GetFirstRowIndex(PassengerDataList);
        var PaymentDataList = book.Worksheet(3);
        FirstRows[2] = GetFirstRowIndex(PaymentDataList);

        Train[] trains = new Train[GetTrainCount(TrainDataList, FirstRows[0])];
        for (int i = 0; i < trains.Length; i++) trains[i] = new Train();
        int counter = 0;
        int refCounter = 0;
        //Достаем Поезда
        foreach (Train train in trains)
        {

            train.Period = $"{year}{month}";
            train.Number = TrainDataList.Cell(FirstRows[0]+counter+refCounter, 2).Value.ToString();
            
            //Станции
            //!!!!Добавить поиск Станции в БД и запрашивать её у пользователя, если таковой не нашлось
            string[] Stations = TrainDataList.Cell(FirstRows[0] + counter + refCounter, 3).Value.ToString().Split(new char[]{'–','-'});
            if (Stations.Length == 2)
            {
                train.StationFrom = new Station(){Name =  Stations[0]};
                train.StationTo = new Station(){Name =  Stations[1]};
            }
            if (Stations.Length == 3)
            {
                train.StationFrom = new Station(){Name =  Stations[0]};
                train.StationMiddle = new Station(){Name =  Stations[1]};
                train.StationTo = new Station(){Name =  Stations[2]};
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
            route.Year = year;
            route.Month = month;
            
            string number = PassengerDataList.Cell(FirstRows[1] + counter, 2).Value.ToString();
            route.RouteNumber = number.ToLower().Contains("ручную") ? "0" : number;
            
            route.train = trains.Where(t => t.Number.Contains(route.RouteNumber)).ToList();
            
            route.Casual = GetCategoryData(counter, 0);
            route.Student = GetCategoryData(counter, 1);
            route.FedBenefit = GetCategoryData(counter, 2);
            route.RegBenefit = GetCategoryData(counter, 3);
            route.Another = GetCategoryData(counter, 4);
            Console.WriteLine($"{route.RouteId} {route.Casual.Count}");
            counter++;
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

    private bool IsDuplicate(int id, IXLWorksheet worksheet)
    {
        if (id < 2) return false;
        
        var previous = worksheet.Cell(id-1, 2).Value.ToString().Replace("*", "").Trim();
        var now = worksheet.Cell(id, 2).Value.ToString().Replace("*", "").Trim();
        
        if (previous == now) return  true;
        return false;
    }
}

