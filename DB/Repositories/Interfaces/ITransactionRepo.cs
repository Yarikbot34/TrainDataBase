using Domain.Classes;

namespace DB.Repositories;

public interface ITransactionRepo
{
    Task WriteNewTransactionAsync(Transaction t);
    Task<List<Transaction>> GetTransactionsByYearAsync(int year);
    Task<List<Transaction>> GetAllTransactionsAsync();
    
}