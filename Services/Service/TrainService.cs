using Domain.Classes;
using DB;
using DB.Repositories;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class TrainService : ITrainService
{
    private readonly ITrainRepo _trainRepo;
    
    public TrainService(ITrainRepo trainRepo)
    {
        _trainRepo = trainRepo;
    }
    
    public async Task<List<TrainDto>> GetTrainsAsync()
    {
        var trains = await _trainRepo.GetAllTrainsAsync();
        var answ = trains
            .Select(t => new TrainDto(t))
            .ToList();
        
        return answ;
    }
    
    public async Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number)
    {
        number = number.Replace("*", "").Trim();
        var answ = await _trainRepo
            .GetAllTrainsByNumberAndYearMonthAsync(year, month, number);
        return answ;
    }

    public async Task AddTrainDescById(int id, TrainDto dto)
    {
        string description = dto.Description;
        Train train = await _trainRepo.GetTrainByIdAsync(id);
        if (train != null)
        {
            train.Description = description;
            await _trainRepo.UpdateTrain(train);
        }
        else throw new Exception("Поезд с таким номером не найден");
    }
}