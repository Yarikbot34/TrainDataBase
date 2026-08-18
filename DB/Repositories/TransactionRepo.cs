using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class TransactionRepo : ITransactionRepo
{
    private readonly AppDbContext ldb;
    
    public TransactionRepo(AppDbContext ldb)
    {
        this.ldb = ldb;
    }

    public async Task WriteNewTransactionAsync(Transaction t)
    {
        ldb.Transactions.Add(t);
        await ldb.SaveChangesAsync();
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        var answ = await ldb.Transactions.ToListAsync();
        return answ;
    }

    public async Task<List<Transaction>> GetTransactionsByYearAsync(int year)
    {
        year = year % 1000;
        var answ = await ldb.Transactions
            .Where(t => t.Year == year)
            .ToListAsync();
        return answ;
    }

    public async Task<Transaction?> GetTransactionByYearAndMonthAsync(int year, int month)
    {
        var answ = await ldb.Transactions
            .FirstOrDefaultAsync(t => t.Year == year && t.Month == month);
        return answ;
    }
}