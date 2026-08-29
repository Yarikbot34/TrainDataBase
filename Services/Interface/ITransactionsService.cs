using Domain.DTO;

namespace Services;

public interface ITransactionsService
{
    Task<List<TransactionDto>> GetTransactionsListAsync(TransactionFilterDto filter);
    Task RemoveUnitsByTransactionIdAsync(int transactionId);
}