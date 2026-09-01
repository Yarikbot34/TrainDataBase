using System.Security.Claims;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;

namespace Services;

public class TransactionsService : ITransactionsService
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IUserService _userService;
    private readonly IUserRepo _userRepo;
    
    public  TransactionsService(ITransactionRepo transactionRepo, IUserService userService, IUserRepo userRepo)
    {
        _transactionRepo = transactionRepo;
        _userService = userService;
        _userRepo = userRepo;
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
            Transaction deleteNote = new Transaction();
            deleteNote.Month = transaction.Month;
            deleteNote.Year = transaction.Year;
            deleteNote.Time = TimeOnly.FromDateTime(DateTime.Now);
            deleteNote.Date = DateOnly.FromDateTime(DateTime.Now);
            deleteNote.Type = Transaction.TransactionType.Delete;
            deleteNote.Description = $"Удаление элементов из файла за {transaction.Month}.{transaction.Year}";
            deleteNote.User = await _userRepo.GetUserByUsernameAsync(auth.Name);
            await _transactionRepo.DeleteTransactionAsync(transaction);
            await _transactionRepo.WriteNewTransactionAsync(deleteNote);
            
        }
    }
}