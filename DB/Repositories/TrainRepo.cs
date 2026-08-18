using Domain.Classes;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class TrainRepo : ITrainRepo
{
    private readonly AppDbContext ldb;
    
    public TrainRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task<Train> GetTrainByIdAsync(int id)
    {
        var answ = await ldb.Trains
            .Where(t => t.Id == id)
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .FirstOrDefaultAsync();
        return answ;
    }
    
    public async Task<List<Train>> GetAllTrainsAsync()
    {
        var answ = await ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .ToListAsync();
        return answ;
    }

    public async Task<List<Train>> GetAllTrainsByYearAsync(List<int> year)
    {
        var answ = await ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .Where(t => year.Contains(t.year))
            .ToListAsync();
        return answ;
    }

    public async Task<List<Train>> GetAllTrainsByNumberAndYearMonthAsync(int year, int month, string number)
    {
        var answ = await ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .Where(t => t.Number == number)
            .Where(t => t.year == year)
            .Where(t => t.month == month)
            .ToListAsync();
        return answ;
    }

    public async Task UpdateTrain(Train train)
    {
        if (train == null || train.Id == null)
        {
            throw new ArgumentException("Обновление пустого значения");
        }
        else
        {
            ldb.Trains.Update(train);
            ldb.SaveChanges();
        }
        return;
    }
}