using Domain.Classes;

namespace Services;

public interface ITrainRepo
{
   Task<List<Train>> GetTrainsByPeriodAndNumber(int year, int month, string number);
}