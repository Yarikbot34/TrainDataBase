using System.Security.Claims;
using DB.Repositories;
using Domain.DTO;

namespace Services;

public class TransactionsService : ITransactionsService
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IUserService _userService;
    private readonly IStationRepo _stationRepo;
    
    public  TransactionsService(ITransactionRepo transactionRepo, IUserService userService, IStationRepo stationRepo)
    {
        _transactionRepo = transactionRepo;
        _userService = userService;
        _stationRepo = stationRepo;
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

    public async Task RemoveUnitsByTransactionAsync(TransactionDeleteDto dto, ClaimsPrincipal user)
    {
        AuthDto auth = new AuthDto
        {
            Name = user.Identity.Name,
            Password = dto.AdminPassword
        };
        if (await _userService.CheckUserAsync(auth))
        { 
            var transaction = await _transactionRepo.GetTransactionByIdAsync(dto.TransactionId);
            if (transaction is null) throw new Exception("Транзакция не найдена");
            await _transactionRepo.DeleteTransactionAsync(transaction, dto.StationDelete);
        }
    }
}