using DB;
using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class RawDataRepo : IRawDataRepo
{
    private AppDbContext _db;
    
    public RawDataRepo(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<Route>> getRoutesAsync(int[] years)
    {
        var answ = _db.Routes
            .Where(t => years.Contains(t.Year))
            .ToList();
        return answ;
    }
    
    public async Task<List<Train>> getTrainsAsync()
    {
        var answ = _db.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationTo)
            .Include(t => t.StationMiddle)
            .ToList();

        return answ;
    }

    public async Task<Train> getTrainByIdAsync(int id)
    {
        return _db.Trains.FirstOrDefault(t => t.Id == id);
    }

    public async Task PatchTrainAsync(Train train)
    {
        _db.Trains.Update(train);
        await _db.SaveChangesAsync();
    }
    
    
    public async Task<List<Station>> getStationsAsync()
    {
        return _db.Stations.ToList();
    }
    
}