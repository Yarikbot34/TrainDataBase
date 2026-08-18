using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface IRouteService
{
    Task<List<RouteDto>> GetRoutesAsync(List<int> years);
    Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter);
}