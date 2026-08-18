using Domain.Classes;

namespace DB.Repositories;

public interface IStationRepo
{
    Task WriteStationAsync(Station station);
    Task WriteStationsAsync(IEnumerable<Station> stations);
    Task<Station> GetStationByIdAsync(int stationId);
    Task<List<Station>> GetAllStationsAsync();
}