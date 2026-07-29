namespace TableReader;

class Program
{
    public static void Main()
    {
        var fs = new FileStream("dataTrue.xlsx", FileMode.Open, FileAccess.Read);
        TrainExtractor te = new TrainExtractor();
        te.Extract(fs, 26, 7);
    }
}