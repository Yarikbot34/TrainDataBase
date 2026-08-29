using Domain.DTO;

namespace Services;

public interface ITransactionsService
{
    Task<List<TransactionDto>> GetTransactionsListAsync(TransactionFilterDto filter);
    Task PatchTransactionDescFromDtoAsync(TransactionDto dto);
    Task RemoveUnitsByTransactionIdAsync(int transactionId);
}