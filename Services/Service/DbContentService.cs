using DB.Repositories;
using DB;
using Domain.Classes;
using Domain.DTO;
using Microsoft.Extensions.Caching.Hybrid;

namespace Services;

public class DbContentService : IdbContentService
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IRouteRepo _routeRepo;
    private readonly IStationRepo _stationRepo;
    private readonly IMapSchemaRepo _mapSchemaRepo;
    private readonly HybridCache _cache;

    public DbContentService
        (IRouteRepo route,  
            IStationRepo stationRepo, 
            ITransactionRepo transactionRepo, 
            IMapSchemaRepo mapSchemaRepo,
            HybridCache cache)
    {
        _routeRepo = route;
        _stationRepo = stationRepo;
        _transactionRepo = transactionRepo;   
        _mapSchemaRepo = mapSchemaRepo;
        _cache = cache;
    }
    
    
    public async Task<List<int>> GetRecordedYearsAsync()
    {
        var key = "recorded:years";

        var answ = await _cache.GetOrCreateAsync(key, async ct => await BuildRecordedYears(), 
            tags: new []{"dbContent", "data"});
        
        return answ;

        
        async Task<List<int>> BuildRecordedYears()
        {
            var transactions = await _transactionRepo.GetAllTransactionsAsync();
            HashSet<int> years = transactions.Select(x => x.Year).ToHashSet();
            return years.ToList();
        }
    }

    public async Task<List<PeriodDto>> GetRecordedPeriodsAsync()
    {
        var key = "recorded:periods";
        
        var answ =  await _cache.GetOrCreateAsync(key, async ct => await BuildRecordedPeriods(),
            tags: new []{"dbContent", "data"});
        
        return answ;
        
        
        async Task<List<PeriodDto>> BuildRecordedPeriods()
        {
            var years = await GetRecordedYearsAsync();
            var periods = new List<PeriodDto>();
            foreach (var year in years)
            {
                var months = await _transactionRepo.GetTransactionsByYearAsync(year);
                var period = new PeriodDto()
                {
                    Year = year,
                    Months = months
                        .Where(t => t.Type != Transaction.TransactionType.Delete &&
                                    t.Type != Transaction.TransactionType.Update)
                        .Select(t => t.Month).ToHashSet().ToList(),
                };
                periods.Add(period);
            }
            return periods;
        }
    }

    public async Task<List<string>> GetRecordedNumbersAsync()
    {
        var key = "recorded:numbers";
        
        var asnw = await _cache.GetOrCreateAsync(key, async ct => await BuildRecordedNumbers(),
            tags: new []{"dbContent", "data", "routes", "trains"});

        return asnw;
        
        
        async Task<List<string>> BuildRecordedNumbers()
        {
            var rawroutes = await _routeRepo.GetAllRoutesAsync();
            HashSet<string> routes = rawroutes.Select(r => r.RouteNumber).ToHashSet();
            return routes.ToList();
        }
    }

    public async Task<List<string>> GetRecordedStationsAsync()
    {
        var key = "recorded:stations";
        
        var answ = await _cache.GetOrCreateAsync(key, async ct => await BuildRecordedStations());
        
        return answ;
        
        
        async Task<List<string>> BuildRecordedStations()
        {
            var stationsRaw = await _stationRepo.GetAllStationsAsync();
            HashSet<string> stations = stationsRaw.Select(s => s.Name).ToHashSet();
            return stations.ToList();
        }
    }

    public async Task<List<string>> GetRecordedSchemasAsync()
    {
        var key = "recorded:schemas";
        
        var answ = await _cache.GetOrCreateAsync(key, async ct => await BuildRecordedSchemas(),
            tags: new []{"dbContent", "map"});
        
        return answ;
        
        
        async Task<List<string>> BuildRecordedSchemas()
        {
            var schemasRaw =  await _mapSchemaRepo.GetAllSchemasAsync();
            HashSet<string> schemas = schemasRaw.Select(s => s.Name).ToHashSet();
            return schemas.ToList();
        }
    }
    
    public async Task<List<string>> GetTransactionTypesAsync()
    {
        return Enum.GetNames(typeof(Transaction.TransactionType)).ToList();
    }
}