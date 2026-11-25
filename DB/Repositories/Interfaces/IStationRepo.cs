using Domain.Classes;
using Domain.Enums;

namespace DB.Repositories;

public interface IStationRepo
{
    Task WriteStationAsync(Station station);
    Task WriteStationsAsync(IEnumerable<Station> stations);
    Task<Station> GetStationByIdAsync(int stationId);
    Task<Station?> GetStationByNameAsync(string name);
    Task<List<Station>> GetAllStationsAsync();
    Task DeleteStationByIdAsync(int stationId);
    Task<StationUpdateResult> UpdateStationAsync(int stationId, string name, string? stationClass);
}