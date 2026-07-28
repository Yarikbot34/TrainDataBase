using System.Runtime.InteropServices.ComTypes;
using ClosedXML.Excel;
using DB;
using DocumentFormat.OpenXml.Spreadsheet;


namespace TableReader;

class TrainExtractor
{
    public void Extract(FileStream fs, string period)
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

        Train[] trains = new Train[GetTrainCount(PassengerDataList, FirstRows[1])];
        for (int i = 0; i < trains.Length; i++) trains[i] = new Train();
        int counter = 0;
        foreach (Train train in trains)
        {
            train.Period = period;
            train.Number = TrainDataList.Cell(FirstRows[0]+counter, 2).Value.ToString();
            
            //Станции
            //!!!!Добавить поиск Станции в БД и запрашивать её у пользователя, если таковой не нашлось
            string[] Stations = TrainDataList.Cell(FirstRows[0] + counter, 3).Value.ToString().Split('-');
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
            string[] Time = TrainDataList.Cell(FirstRows[0] + counter, 4).Value.ToString().Split('–');
            train.TimeFrom = TimeOnly.Parse(Time[0]);
            train.TimeFrom = TimeOnly.Parse(Time[1]);
            
            //Метрики
            train.Distance = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter, 5).Value.ToString());
            train.RailcarCount = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter, 6).Value.ToString());
            train.RangePerDay = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter, 7).Value.ToString());
            train.Distance = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter, 8).Value.ToString());
            train.RangePerMonth = Convert.ToInt32(TrainDataList.Cell(FirstRows[0] + counter, 9).Value.ToString());

            Console.WriteLine($"{train.Number}|{train.Distance}");
            counter++;
            
        }

    }

    private int GetFirstRowIndex(IXLWorksheet worksheet)
    {
        int counter = 0;
        string value = "";
        while (value != "1")
        {
            counter++;
            IXLCell cell = worksheet.Cell(counter, 1);
            value = cell.Value.ToString();
        }
        return counter;
    }

    private int GetTrainCount(IXLWorksheet worksheet, int TableStart)
    {
        int counter = TableStart;
        string value = "1";
        string id = "1";
        while (int.TryParse(id, out _) && int.TryParse(value, out _))
        {
            counter++;
            id = worksheet.Cell(counter, 1).Value.ToString();
            value = worksheet.Cell(counter, 2).Value.ToString().Split('/')[0];
        }
        return Convert.ToInt32(id)-1;
    }
}

