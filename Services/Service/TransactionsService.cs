using System.Security.Authentication;
using System.Security.Claims;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Microsoft.Extensions.Caching.Hybrid;

namespace Services;

public class TransactionsService : ITransactionsService
{
    private readonly ITransactionRepo _transactionRepo;
    private readonly IUserService _userService;
    private readonly IUserRepo _userRepo;
    private readonly HybridCache _cache;
    
    public  TransactionsService(ITransactionRepo transactionRepo, IUserService userService, IUserRepo userRepo, 
        HybridCache cache)
    {
        _transactionRepo = transactionRepo;
        _userService = userService;
        _userRepo = userRepo;
        _cache = cache;
    }
    
    public async Task<List<TransactionDto>> GetTransactionsListAsync(TransactionFilterDto filter)
    {
        var transactions = await _transactionRepo.GetTransactionsByFilterAsync(filter);
        var answ = transactions.Select(x => new TransactionDto(x)).ToList();
        return answ;
    }

    public async Task PatchTransactionDescFromDtoAsync(TransactionDto dto)
    {
        int id = dto.Id;
        var transaction = await _transactionRepo.GetTransactionByIdAsync(id);
        
        if (transaction is null) throw new Exception($"Транзакция №{id} не обнаружена в системе");
        
        transaction.Description = dto.Description;
        
        await _transactionRepo.PathTransactionAsync(transaction);
    }

    public async Task RemoveUnitsByTransactionAsync(TransactionDeleteDto dto, ClaimsPrincipal user)
    {
        if (user.Identity is null || user.Identity.Name is null) 
            throw new AuthenticationException("Ошибка итендификации");
        
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
            await _cache.RemoveByTagAsync("data");

        }
        else throw new Exception("Неверный пароль. Отказано в доступе");
    }
}