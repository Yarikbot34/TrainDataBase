using Domain.Classes;
using DB;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class TrainService : ITrainService
{
    AppDbContext ldb;
    
    public TrainService(AppDbContext db)
    {
        ldb = db;
    }
    
    public async Task<List<TrainDto>> GetTrainsAsync()
    {
        var answ = ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationTo)
            .Include(t => t.StationMiddle)
            .Select(t => new TrainDto(t))
            .ToList();

        return answ;
    }
    
    public async Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number)
    {
        number = number.Replace("*", "").Trim();
        var answ = await ldb.Trains
            .Where(t => t.year == year && t.month == month && t.Number.Contains(number))
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .OrderBy(t => t.HasDesc)
            .ToListAsync();
        return answ;
    }

    public async Task AddTrainDescById(int id, TrainDto dto)
    {
        string description = dto.Description;
        Train train = ldb.Trains.FirstOrDefault(t => t.Id == id);
        if (train != null)
        {
            train.Description = description;
            ldb.Trains.Update(train);
            await ldb.SaveChangesAsync();
        }
        else throw new Exception("Поезд с таким номером не найден");
    }
}