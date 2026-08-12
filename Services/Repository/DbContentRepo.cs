using DB;

namespace Services;

public class DbContentRepo : IdbContentRepo
{
    private readonly AppDbContext ldb;

    public DbContentRepo(AppDbContext db)
    {
        ldb = db;
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
        HashSet<string> routes = ldb.Routes.Select(r => r.RouteNumber).ToHashSet();
        return routes.ToList();
    }

    public async Task<List<string>> GetRecordedStationsAsync()
    {
        HashSet<string> stations = ldb.Stations.Select(s => s.Name).ToHashSet();
        return stations.ToList();
    }
}