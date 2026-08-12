using Domain.Classes;
using DB;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class TrainRepo : ITrainRepo
{
    AppDbContext ldb;
    
    public TrainRepo(AppDbContext db)
    {
        ldb = db;
    }
    
    public async Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number)
    {
        number = number.Replace("*", "").Trim();
        string period = $"{year}{month}";
        var answ = await ldb.Trains.ToListAsync();
        answ = answ.Where(t => t.Period == period && t.Number.Contains(number)).ToList();
        Console.WriteLine(answ.Count);
        return answ;
    }

    public async Task AddTrainDescById(int id, TrainDto dto)
    {
        string description = dto.Description;
        Train train = ldb.Trains.FirstOrDefault(t => t.Id == id);
        train.Description = description;
        ldb.Trains.Update(train);
        ldb.SaveChanges();
    }
}