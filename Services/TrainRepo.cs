using Domain.Classes;
using DB;
using DB.Repositories;

namespace Services;

public class TrainRepo : ITrainRepo
{
    IRawDataRepo repo;
    
    public TrainRepo(IRawDataRepo repository)
    {
        repo = repository;
    }
    
    public async Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number)
    {
        number = number.Replace("*", "").Trim();
        string period = $"{year}{month}";
        var answ = await repo.getTrainsAsync();
        answ = answ.Where(t => t.Period == period && t.Number.Contains(number)).ToList();
        Console.WriteLine(answ.Count);
        return answ;
    }
}