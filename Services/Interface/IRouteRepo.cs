using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface IRouteRepo
{
    Task<List<RouteDto>> GetRoutesAsync(int[] years);
    Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter);
}