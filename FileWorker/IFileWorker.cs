using Domain.Classes;
using Domain.DTO;

namespace FileWorker;

public interface ITableReader
{
    Task<List<TrainDto>> ExtractFromFile(FileStream fs, int year, int month);
}