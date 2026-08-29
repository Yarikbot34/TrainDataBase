using DB.Repositories;
using Domain.DTO;

namespace Services;

public class TransactionsService : ITransactionsService
{
    private readonly ITransactionRepo _transactionRepo;
    
    public  TransactionsService(ITransactionRepo transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    public async Task<List<TransactionDto>> GetTransactionsListAsync(TransactionFilterDto filter)
    {
        var transactions = await _transactionRepo.GetTransactionsByFilterAsync(filter);
        var answ = transactions.Select(x => new TransactionDto(x)).ToList();
        return answ;
    }

    public Task RemoveUnitsByTransactionIdAsync(int transactionId)
    {
        return Task.CompletedTask;
    }
}