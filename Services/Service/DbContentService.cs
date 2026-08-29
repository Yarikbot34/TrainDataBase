using DB.Repositories;
using DB;
using Domain.Classes;
using Domain.DTO;

namespace Services;

public class DbContentService : IdbContentService
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IRouteRepo _routeRepo;
    private readonly IStationRepo _stationRepo;
    private readonly IMapSchemaRepo _mapSchemaRepo;

    public DbContentService
        (IRouteRepo route,  IStationRepo stationRepo, ITransactionRepo transactionRepo, IMapSchemaRepo mapSchemaRepo)
    {
        _routeRepo = route;
        _stationRepo = stationRepo;
        _transactionRepo = transactionRepo;   
        _mapSchemaRepo = mapSchemaRepo;
    }
    
    
    public async Task<List<int>> GetRecordedYearsAsync()
    {
        var transactions = await _transactionRepo.GetAllTransactionsAsync();
        HashSet<int> years = transactions.Select(x => x.Year).ToHashSet();
        return years.ToList();
    }

    public async Task<List<PeriodDto>> GetRecordedPeriodsAsync()
    {
        var years = await GetRecordedYearsAsync();
        var answ = new List<PeriodDto>();
        foreach (var year in years)
        {
            var months = await _transactionRepo.GetTransactionsByYearAsync(year);
            var period = new PeriodDto()
            {
                Year = year,
                Months = months.Select(t => t.Month).ToHashSet().ToList(),
            };
            answ.Add(period);
        }
        return answ;
    }

    public async Task<List<string>> GetRecordedNumbersAsync()
    {
        var rawroutes = await _routeRepo.GetAllRoutesAsync();
        HashSet<string> routes = rawroutes.Select(r => r.RouteNumber).ToHashSet();
        return routes.ToList();
    }

    public async Task<List<string>> GetRecordedStationsAsync()
    {
        var stationsRaw = await _stationRepo.GetAllStationsAsync();
        HashSet<string> stations = stationsRaw.Select(s => s.Name).ToHashSet();
        return stations.ToList();
    }

    public async Task<List<string>> GetRecordedSchemasAsync()
    {
        var schemasRaw =  await _mapSchemaRepo.GetAllSchemasAsync();
        HashSet<string> schemas = schemasRaw.Select(s => s.Name).ToHashSet();
        return schemas.ToList();
    }
    
    public async Task<List<string>> GetTransactionTypesAsync()
    {
        return Enum.GetNames(typeof(Transaction.TransactionType)).ToList();
    }
}