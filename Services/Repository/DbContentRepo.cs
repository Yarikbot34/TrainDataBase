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
}