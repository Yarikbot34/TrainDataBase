using DB;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
namespace Services;

public class StationService : IStationService
{
    private readonly IStationRepo _stationRepo;

    public StationService(IStationRepo stationRepo)
    {
        _stationRepo = stationRepo;
    }
    
    
    public async Task<List<Station>> GetStationsAsync()
    {
        return await _stationRepo.GetAllStationsAsync();
    }

    public async Task WriteStationFromDtoAsync(StationDto dto)
    {
        var station = dto.ToStation();
        await _stationRepo.WriteStationAsync(station);
    }

    public async Task DeleteStationByIdAsync(int id)
    {
        await _stationRepo.DeleteStationByIdAsync(id);
    }
}