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
}

