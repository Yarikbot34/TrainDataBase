using DB.Repositories;
using DB;

namespace Services;

public class DbContentService : IdbContentService
{
    private readonly AppDbContext ldb;
    private readonly IRouteRepo routeRepo;

    public DbContentService(AppDbContext db,  IRouteRepo route)
    {
        ldb = db;
        routeRepo = route;
    }
    
    
    public async Task<List<int>> GetRecordedYearsAsync()
    {
        HashSet<int> years = ldb.Transactions.Select(x => x.Year).ToHashSet();
        return years.ToList();
    }

    public async Task<List<int>> GetRecordedMonthsAsync()
    {
        HashSet<int> months = ldb.Transactions.Select(x => x.Month).ToHashSet();
        return months.ToList();
    }

    public async Task<List<string>> GetRecordedNumbersAsync()
    {
        var rawroutes = await routeRepo.GetAllRoutesAsync();
        HashSet<string> routes = rawroutes.Select(r => r.RouteNumber).ToHashSet();
        return routes.ToList();
    }

    public async Task<List<string>> GetRecordedStationsAsync()
    {
        HashSet<string> stations = ldb.Stations.Select(s => s.Name).ToHashSet();
        return stations.ToList();
    }

    public async Task<List<string>> GetRecordedSchemasAsync()
    {
        HashSet<string> schemas = ldb.MapSchemas.Select(s => s.Name).ToHashSet();
        return schemas.ToList();
    }
}