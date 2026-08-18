using Domain.Classes;

namespace DB.Repositories;

public interface IStationRepo
{
    Task<Station> GetStationByIdAsync(int stationId);
    Task<List<Station>> GetAllStationsAsync();
}