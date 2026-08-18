using Domain.Classes;
using Domain.DTO;

namespace DB.Repositories;

public interface ITrainRepo
{
    Task<Train> GetTrainByIdAsync(int id);
    Task<List<Train>> GetAllTrainsAsync();
    Task<List<Train>> GetAllTrainsByYearAsync(List<int> year);
    Task<List<Train>> GetAllTrainsByNumberAndYearMonthAsync(int year, int month, string number);
    
    Task UpdateTrain(Train train);
}