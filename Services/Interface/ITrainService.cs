using System.Security.Claims;
using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface ITrainService
{
   Task<List<TrainDto>> GetTrainsAsync();
   Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number);
   Task AddTrainDescById(int id, TrainDto dto, ClaimsPrincipal user);
}