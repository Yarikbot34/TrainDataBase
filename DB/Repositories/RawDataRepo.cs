using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class RawDataRepo : IRawDataRepo
{
    public async Task<List<Route>> getRoutesAsync()
    {
        using AppDbContext db = new AppDbContext();
        return db.Routes.ToList();
    }
    
    public async Task<List<Train>> getTrainsAsync()
    {
        using AppDbContext db = new AppDbContext();
        var answ = db.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationTo)
            .Include(t => t.StationMiddle)
            .ToList();

        return answ;
    }
    
    public async Task<List<Station>> getStationsAsync()
    {
        using AppDbContext db = new AppDbContext();
        return db.Stations.ToList();
    }
    
}