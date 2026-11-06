using System.Security.Claims;
using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface ITrainService
{
   Task<TrainsWithSummDto> GetTrainsAsync();
   Task<TrainsWithSummDto> GetTrainsByFilterAsync(TrainFilterDto filter);
   Task<List<TrainDto>> GetTrainsSortedByNumberAndDescriptionAsync();
   Task<List<TrainDto>> GetTrainsByPeriodAndNumber(int year, int month, string number);
   Task AddTrainDescById(int id, TrainDto dto, ClaimsPrincipal user);
}
