using System.Runtime.InteropServices.ComTypes;
using ClosedXML.Excel;


namespace TableReader;

class TrainExtractor
{
    public void Extract(FileStream fs)
    {
        using var book = new XLWorkbook(fs);
        
        if (book.Worksheets.Count != 3) return;
        
        var TrainDataList = book.Worksheet(1);
        var PassengerDataList = book.Worksheet(2);
        var PaymentDataList = book.Worksheet(3);
        
        
        
        
    }
}

