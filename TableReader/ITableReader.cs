using Domain.Classes;
using Domain.DTO;

namespace TableReader;

public interface ITableReader
{
    Task<List<TrainDto>> ExtractFromFile(FileStream fs, int year, int month);
}