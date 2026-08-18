using DB;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
namespace Services;

public class StationService : IStationService
{
    private readonly IStationRepo _stationRepo;

    public StationService(AppDbContext db, IStationRepo stationRepo)
    {
        _stationRepo = stationRepo;
    }
    
    
    public async Task<List<Station>> GetStationsAsync()
    {
        return await _stationRepo.GetAllStationsAsync();
    }
}