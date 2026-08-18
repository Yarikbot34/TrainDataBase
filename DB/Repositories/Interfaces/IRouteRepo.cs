using Domain.Classes;
namespace DB.Repositories;

public interface IRouteRepo
{
    Task<List<Route>> GetAllRoutesAsync();
    Task<List<Route>> GetAllRoutesWithTrainsAsync();
    Task<Route> GetRouteByIdAsync(int routeId);
    Task<List<Route>> GetRoutesByYearListAsync(List<int> years);
}