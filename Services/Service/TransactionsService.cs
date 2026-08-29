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

    public async Task PatchTransactionDescFromDtoAsync(TransactionDto dto)
    {
        int Id = dto.Id;
        var transaction = await _transactionRepo.GetTransactionByIdAsync(Id);
        
        if (transaction is null) throw new Exception($"Транзакция №{Id} не обнаружена в системе");
        
        transaction.Description = dto.Description;
        
        await _transactionRepo.PathTransactionAsync(transaction);
    }

    public Task RemoveUnitsByTransactionIdAsync(int transactionId)
    {
        return Task.CompletedTask;
    }
}