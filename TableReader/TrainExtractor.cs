using System.Runtime.InteropServices.ComTypes;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;


namespace TableReader;

class TrainExtractor
{
    public void Extract(FileStream fs)
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
        
        Console.WriteLine(GetTrainCount(PassengerDataList, FirstRows[1]));
        
        
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

