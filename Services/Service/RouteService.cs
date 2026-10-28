using DB;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Services;

public class RouteService : IRouteService
{
    private readonly IRouteRepo _routeRepo;
    private readonly HybridCache _cache;
    public RouteService(IRouteRepo routeRepo, HybridCache cache)
    {
        _routeRepo = routeRepo;
        _cache = cache;
    }

    public async Task<RoutesWithSummDto> GetRoutesWithSummAsync(List<int> years)
    {
        var key = "routes:withSumm";
        
        var asnw = await _cache.GetOrCreateAsync(
            key,
            async ct => await BuildRoutesWithSumm());
        
        return asnw;

        async Task<RoutesWithSummDto> BuildRoutesWithSumm()
        {
            var rotes = await GetRoutesAsync(years);
            var dto = new RoutesWithSummDto(rotes);
            return dto;
        }
    }
    
    public async Task<RoutesWithSummDto> GetRoutesByFilterWithSummAsync(RouteFilterDto filter)
    {
        var rotes = await GetRoutesByFilter(filter);
        var answ = new RoutesWithSummDto(rotes);
        return answ;
    }

    public Task<List<RouteDto>> GetRoutesAsync(List<int> years)
    {
        var answ = _routeRepo.GetRoutesByYearListAsync(years).Result
            .Select(r => new RouteDto(r))
            .ToList();
        return Task.FromResult(answ);
    }
    
    public async Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter)
    {
        var routes = await _routeRepo.GetRoutesByFilterAsync(filter);
        var answ = routes.Select(r => new RouteDto(r)).ToList();
        return answ;
    }
}