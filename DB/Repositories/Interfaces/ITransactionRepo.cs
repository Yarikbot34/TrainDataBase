using Domain.Classes;
using Domain.DTO;

namespace DB.Repositories;

public interface ITransactionRepo
{
    Task WriteNewTransactionAsync(Transaction t);
    Task<List<Transaction>> GetTransactionsByYearAsync(int year);
    Task<List<Transaction>> GetTransactionsByFilterAsync(TransactionFilterDto filter);
    Task<List<Transaction>> GetAllTransactionsAsync();
    Task<Transaction?> GetTransactionByYearAndMonthAsync(int year, int month);
    
}