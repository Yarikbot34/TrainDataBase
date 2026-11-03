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
    
    public TrainService(ITrainRepo trainRepo, ITransactionRepo transactionRepo, IUserRepo userRepo)
    {
        _trainRepo = trainRepo;
        _transactionRepo = transactionRepo;
        _userRepo = userRepo;
    }
    
    public async Task<TrainsWithSummDto> GetTrainsAsync()
    {
        var trains = await _trainRepo.GetAllTrainsAsync();
        var lastMonth = DateTime.Today.AddMonths(-11);
        var filtered = trains.Where(t => IsInLastTwelveMonths(t.year, t.month, lastMonth)).ToList();
        var dto = filtered
            .Select(t => new TrainDto(t))
            .ToList();
        return new TrainsWithSummDto(dto);

        static bool IsInLastTwelveMonths(int year, int month, DateTime firstMonth)
        {
            var normalizedYear = year < 1000 ? 2000 + year : year;
            var value = new DateTime(normalizedYear, month, 1);
            var lastMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return value >= new DateTime(firstMonth.Year, firstMonth.Month, 1) && value <= lastMonth;
        }
    }

    public async Task<List<TrainDto>> GetTrainsSortedByNumberAndDescriptionAsync()
    {
        var trains = await _trainRepo.GetAllTrainsAsync();

        return trains
            .Select(t => new TrainDto(t))
            .OrderBy(t => GetNumberSortKey(t.Number))
            .ThenBy(t => string.IsNullOrWhiteSpace(t.Description))
            .ThenBy(t => t.RowInFile)
            .ToList();

        static (bool IsNumber, int Number, string Text) GetNumberSortKey(string number)
        {
            var normalized = number.Replace("*", "").Trim();
            return (int.TryParse(normalized, out var numericNumber),
                numericNumber,
                normalized);
        }
    }
    
    public async Task<List<TrainDto>> GetTrainsByPeriodAndNumber(int year, int month, string number)
    {
        number = number.Replace("*", "").Trim();
        var trains = await _trainRepo
            .GetAllTrainsByNumberAndYearMonthAsync(year, month, number);
        var answ = trains.Select(t => new TrainDto(t))
            .OrderBy(t => t.RowInFile)
            .ToList();
        return answ;
    }

    public async Task AddTrainDescById(int id, TrainDto dto, ClaimsPrincipal user)
    {
        if (user.Identity is null) 
            throw new Exception("Не удалось авторизовать пользователя");
        var transaction = await _transactionRepo.GetTransactionByYearAndMonthAsync(dto.Year, dto.Month, true);
        if (transaction is not null && transaction.User is not null)
        {
            DateTime transactionTime = transaction.Date.ToDateTime(transaction.Time).ToUniversalTime();
            if (DateTime.UtcNow - transactionTime < TimeSpan.FromHours(2) &&
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
