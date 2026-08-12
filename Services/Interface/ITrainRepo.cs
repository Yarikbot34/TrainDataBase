using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface ITrainRepo
{
   Task<List<TrainDto>> GetTrainsAsync();
   Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number);
   Task AddTrainDescById(int id, TrainDto dto);
}