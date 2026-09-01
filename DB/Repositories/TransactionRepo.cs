using Domain.Classes;
using Domain.DTO;
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

    public async Task<List<Transaction>> GetTransactionsByFilterAsync(TransactionFilterDto filter)
    {
        List<Transaction> transactions;
        if (filter.Periods is not null && filter.Periods.Count > 0)
        {
            transactions  = await ldb.Transactions
                .Where(t => filter.Periods.Any(p => p.Months.Contains(t.Month) && p.Year == t.Year))
                .Include(t => t.User)
                .ToListAsync();
        }
        else
        {
            transactions  = await ldb.Transactions
                .Include(t => t.User)
                .ToListAsync();
        }
            
        if (filter.UserNames is not null && filter.UserNames.Count > 0) transactions = transactions
            .Where(t => filter.UserNames.Contains(t.User.Username)).ToList();
        if (filter.TransactionTypes is not null && filter.TransactionTypes.Count > 0) transactions = transactions
            .Where(t => filter.TransactionTypes.Contains(t.Type.ToString())).ToList();
        if (filter.EndDate is not null && filter.StartDate is not null && filter.EndDate > filter.StartDate)
        {
            transactions = transactions
                .Where(t => filter.EndDate.Value <= t.Date.ToDateTime(t.Time) )
                .Where(t => filter.StartDate.Value >= t.Date.ToDateTime(t.Time)).ToList();
        }
        return transactions;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await ldb.Transactions.FindAsync(id);
    }
    
    public async Task<Transaction?> GetTransactionByYearAndMonthAsync(int year, int month, bool getUser = false)
    {
        Transaction? answ;
        if (getUser)
        {
            answ = await ldb.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Year == year && t.Month == month);
        }
        else
        {
            answ = await ldb.Transactions
                .FirstOrDefaultAsync(t => t.Year == year && t.Month == month);
        }
        
        return answ;
    }

    public async Task PathTransactionAsync(Transaction t)
    {
        ldb.Transactions.Update(t);
        await ldb.SaveChangesAsync();
    }

    public async Task DeleteTransactionAsync(Transaction t)
    {
        if (t.Type == Transaction.TransactionType.AddFile)
        {
            ldb.Transactions
                .RemoveRange(ldb.Transactions.Where(tr => tr.Month == t.Month && 
                                                          tr.Year == t.Year && 
                                                          tr.Type != Transaction.TransactionType.Add));
        }
        else ldb.Transactions.Remove(t);
        await ldb.SaveChangesAsync();
    }
}