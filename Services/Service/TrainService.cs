using System.Diagnostics.Metrics;
using System.Security.Claims;
using Domain.Classes;
using DB;
using DB.Repositories;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class TrainService : ITrainService
{
    private readonly ITrainRepo _trainRepo;
    private readonly ITransactionRepo _transactionRepo;
    private readonly IUserRepo _userRepo;
    
    public TrainService(ITrainRepo trainRepo, ITransactionRepo transactionRepo)
    {
        _trainRepo = trainRepo;
        _transactionRepo = transactionRepo;
    }
    
    public async Task<List<TrainDto>> GetTrainsAsync()
    {
        var trains = await _trainRepo.GetAllTrainsAsync();
        var answ = trains
            .Select(t => new TrainDto(t))
            .ToList();
        
        return answ;
    }
    
    public async Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number)
    {
        number = number.Replace("*", "").Trim();
        var answ = await _trainRepo
            .GetAllTrainsByNumberAndYearMonthAsync(year, month, number);
        return answ;
    }

    public async Task AddTrainDescById(int id, TrainDto dto, ClaimsPrincipal user)
    {
        if (user.Identity is null) throw new Exception("Не удалось авторизовать пользователя");
        var transaction = await _transactionRepo.GetTransactionByYearAndMonthAsync(dto.Year, dto.Month);
        if (transaction is not null)
        {
            if (transaction.Date == DateOnly.FromDateTime(DateTime.Now) &&
                TimeOnly.FromDateTime(DateTime.Now) - transaction.Time < TimeSpan.FromHours(2) &&
                transaction.User is not null &&
                user.Identity.Name == transaction.User.Username)
            {
                await WriteDto();
            }
            else
            {
                User reqUser = await _userRepo.GetUserByUsernameAsync(user.Identity.Name);
                if (reqUser is not null)
                {
                    await WriteDto();
                    
                    Transaction note = new Transaction();
                    note.Date = DateOnly.FromDateTime(DateTime.Now);
                    note.Time = TimeOnly.FromDateTime(DateTime.Now);
                    note.User = reqUser;
                    note.Type = Transaction.TransactionType.Update;
                    note.Year = dto.Year;
                    note.Month = dto.Month;
                    note.Description = $"Добавлено/Отредактировано описание для поезда {dto.Number} в {dto.Month}.{dto.Year}";
                    note.UnitsGet = 1;
                    await _transactionRepo.WriteNewTransactionAsync(note);
                }
            }

            async Task WriteDto()
            {
                string description = dto.Description;
                Train train = await _trainRepo.GetTrainByIdAsync(id);
                if (train != null)
                {
                    train.Description = description;
                    await _trainRepo.UpdateTrain(train);
                }
                else throw new Exception("Поезд с таким номером не найден");
            }
        }
    }

}