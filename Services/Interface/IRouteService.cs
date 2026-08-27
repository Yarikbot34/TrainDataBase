using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface IRouteService
{
    Task<RoutesWithSummDto> GetRoutesWithSummAsync(List<int> years);
    Task<RoutesWithSummDto> GetRoutesByFilterWithSummAsync(RouteFilterDto filter);
    Task<List<RouteDto>> GetRoutesAsync(List<int> years);
    Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter);
}