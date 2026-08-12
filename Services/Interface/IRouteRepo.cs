using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface IRouteRepo
{
    Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter);
}