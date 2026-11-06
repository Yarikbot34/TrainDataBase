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

    public async Task<List<Train>> GetTrainsByFilterAsync(TrainFilterDto filter)
    {
        var query = ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .AsQueryable();

        if (filter.numbers is { Count: > 0 })
        {
            var numbers = filter.numbers
                .Where(number => !string.IsNullOrWhiteSpace(number))
                .Select(number => number.Trim())
                .ToList();
            query = query.Where(t => numbers.Contains(t.Number));
        }

        if (filter.StationsFrom is { Count: > 0 })
        {
            var stationsFrom = filter.StationsFrom
                .Where(station => !string.IsNullOrWhiteSpace(station))
                .Select(station => station.Trim())
                .ToList();
            query = query.Where(t => stationsFrom.Contains(t.StationFrom.Name));
        }

        if (filter.StationsTo is { Count: > 0 })
        {
            var stationsTo = filter.StationsTo
                .Where(station => !string.IsNullOrWhiteSpace(station))
                .Select(station => station.Trim())
                .ToList();
            query = query.Where(t => stationsTo.Contains(t.StationTo.Name));
        }

        var trains = await query.ToListAsync();

        // EF Core не может перевести фильтрацию по вложенной коллекции Months
        // в SQL, поэтому фильтр периода применяется после загрузки данных.
        if (filter.period is { Count: > 0 })
        {
            trains = trains
                .Where(t => filter.period.Any(p =>
                    p.Year == t.year &&
                    p.Months.Contains(t.month)))
                .ToList();
        }

        return trains;
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

    public async Task<List<Train>> GetTrainsByPeriodAsync(List<PeriodDto> periods)
    {
        var raw = await ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .AsNoTracking().ToListAsync();
            
        var answ = raw.Where(t => periods.Any(p => 
                (p.Year == t.year) && 
                (p.Months.Contains(t.month))
                ))
            .ToList();
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
