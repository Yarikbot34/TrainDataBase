namespace TableReader;

public interface ITableReader
{
    Task ExtractFromFile(FileStream fs, int year, int month);
}