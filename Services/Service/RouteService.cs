using DB;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class RouteService : IRouteService
{
    private readonly IRouteRepo _routeRepo;
    
    public RouteService(IRouteRepo routeRepo)
    {
        _routeRepo = routeRepo;
    }

    public async Task<RoutesWithSummDto> GetRoutesWithSummAsync(List<int> years)
    {
        var rotes = await GetRoutesAsync(years);
        var answ = new RoutesWithSummDto(rotes);
        return answ;
    }
    
    public async Task<RoutesWithSummDto> GetRoutesByFilterWithSummAsync(RouteFilterDto filter)
    {
        var rotes = await GetRoutesByFilter(filter);
        var answ = new RoutesWithSummDto(rotes);
        return answ;
    }

    public async Task<List<RouteDto>> GetRoutesAsync(List<int> years)
    {
        var answ = _routeRepo.GetRoutesByYearListAsync(years).Result
            .Select(r => new RouteDto(r))
            .ToList();
        return answ;
    }
    
    public async Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter)
    {
        var routes = await _routeRepo.GetRoutesByFilterAsync(filter);
        var answ = routes.Select(r => new RouteDto(r)).ToList();
        return answ;
    }
}