using Domain.Classes;

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
        return db.Trains.ToList();
    }
    
    public async Task<List<Station>> getStationsAsync()
    {
        using AppDbContext db = new AppDbContext();
        return db.Stations.ToList();
    }
    
}