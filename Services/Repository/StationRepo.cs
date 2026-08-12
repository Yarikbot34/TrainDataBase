using DB;
using Domain.Classes;
using Domain.DTO;
namespace Services;

public class StationRepo : IStationRepo
{
    private readonly AppDbContext ldb;

    public StationRepo(AppDbContext db)
    {
        ldb = db;
    }
    
    
    public async Task<List<Station>> GetStationsAsync()
    {
        return ldb.Stations.ToList();
    }
}