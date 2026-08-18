using Domain.Classes;
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

    public async Task<List<Route>> GetRoutesByYearListAsync(List<int> years)
    {
        var answ = await ldb.Routes
            .Where(r => years.Contains(r.Year))
            .ToListAsync();
        return answ;
    }
}