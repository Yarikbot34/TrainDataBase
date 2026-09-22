using Domain.Classes;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class RouteRepo : IRouteRepo
{
    private readonly AppDbContext ldb;

    public RouteRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task WriteRouteAsync(Route route)
    {
        ldb.Routes.Add(route);
        await ldb.SaveChangesAsync();
    }

    public async Task WriteRoutesAsync(IEnumerable<Route> routes)
    {
        ldb.Routes.AddRange(routes);
        await ldb.SaveChangesAsync();
    }

    public async Task<List<Route>> GetAllRoutesAsync()
    {
        var asnw = await ldb.Routes.ToListAsync();
        return asnw;
    }

    public async Task<List<Route>> GetAllRoutesWithTrainsAsync()
    {
        var answ = await ldb.Routes
            .Include(r => r.Trains)
            .ThenInclude(t => t.StationFrom)
            .Include(r => r.Trains)
            .ThenInclude(t => t.StationTo)
            .Include(r => r.Trains)
            .ThenInclude(t => t.StationMiddle)
            .ToListAsync();
        return answ;
    }

    public async Task<Route> GetRouteByIdAsync(int routeId)
    {
        var answ = await ldb.Routes
            .FirstOrDefaultAsync(r => r.RouteId == routeId);
        return answ;
    }

    public async Task<List<Route>> GetRoutesByFilterAsync(RouteFilterDto filter)
    {
        var routeList = GetAllRoutesWithTrainsAsync()
            .Result.AsQueryable();
        
        var answ = ApplyFilter(filter, routeList);  
        
        return answ.ToList();

        IQueryable<Route> ApplyFilter(RouteFilterDto filter, IQueryable<Route> query)
        {
            if (filter.period is not null)
            {
                query = query.Where(r => filter.period
                    .Any(p => p.Months.Contains(r.Month) && p.Year == r.Year));
            }
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

    public async Task<List<Route>> GetRoutesByYearListAsync(List<int> years, bool includeTrains = false)
    {
        List<Route> answ = null;
        if (includeTrains)
        {
            answ = await ldb.Routes
                .Where(r => years.Contains(r.Year))
                .Include(r => r.Trains)
                .ThenInclude(t => t.StationFrom)
                .Include(r => r.Trains)
                .ThenInclude(t => t.StationTo)
                .Include(r => r.Trains)
                .ThenInclude(t => t.StationMiddle)
                .ToListAsync();
        }
        else
        {
            answ = await ldb.Routes
                .Where(r => years.Contains(r.Year))
                .ToListAsync();   
        }
        return answ;
    }
}