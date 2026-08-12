using DB;
using Domain.Classes;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class RouteRepo : IRouteRepo
{
    private readonly AppDbContext ldb;
    
    public RouteRepo(AppDbContext ldb)
    {
        this.ldb = ldb;
    }

    public async Task<List<RouteDto>> GetRoutesAsync(int[] years)
    {
        var answ = ldb.Routes
            .Where(t => years.Contains(t.Year))
            .Select(r => new RouteDto(r))
            .ToList();
        return answ;
    }
    
    public async Task<List<RouteDto>> GetRoutesByFilter(RouteFilterDto filter)
    {
        var routeList = ldb.Routes
            .Include(r => r.Trains)
            .ThenInclude(t => t.StationFrom)
            .Include(r => r.Trains)
            .ThenInclude(t => t.StationTo)
            .AsQueryable();
        
        routeList = ApplyFilter(filter, routeList);  
        
        var answ = routeList.Select(r => new RouteDto(r)).ToList();

        return answ;

        IQueryable<Route> ApplyFilter(RouteFilterDto filter, IQueryable<Route> query)
        {
            if (filter.year != null) query = query.Where(r => r.Year == filter.year);
            if (filter.month != null) query = query.Where(r => r.Month == filter.month);     
            if (filter.number != null) query = query.Where(r => r.RouteNumber.Contains(filter.number.Trim()));
            
            if (filter.stationFrom != null)
            {
                query = query.Where(r => r.Trains.Any(t => 
                    t.StationFrom.Name == filter.stationFrom));
            }

            if (filter.stationTo != null)
            {
                query = query.Where(r => r.Trains.Any(t => 
                    t.StationTo.Name == filter.stationTo));
            }
            return query;
        }
    }
}