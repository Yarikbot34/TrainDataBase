using DB;
using Domain.Classes;
using Domain.DTO;
namespace Services;

public class StationService : IStationService
{
    private readonly AppDbContext ldb;

    public StationService(AppDbContext db)
    {
        ldb = db;
    }
    
    
    public async Task<List<Station>> GetStationsAsync()
    {
        return ldb.Stations.ToList();
    }
}