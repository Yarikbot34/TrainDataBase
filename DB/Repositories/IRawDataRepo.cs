using Domain.Classes;

namespace DB.Repositories;

public interface IRawDataRepo
{
    Task<List<Route>> getRoutesAsync();
    Task<List<Train>> getTrainsAsync();
    Task<List<Station>> getStationsAsync();
}