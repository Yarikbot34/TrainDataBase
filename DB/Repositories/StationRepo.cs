using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class StationRepo : IStationRepo
{
    private readonly AppDbContext ldb;
    
    public StationRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task<Station> GetStationByIdAsync(int stationId)
    {
        return await ldb.Stations.FindAsync(stationId);
    }

    public async Task<List<Station>> GetAllStationsAsync()
    {
        return await ldb.Stations.ToListAsync();
    }
}