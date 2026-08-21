using Domain.DTO;

namespace Services;

public interface IdbContentService
{
   Task<List<int>> GetRecordedYearsAsync();
   Task<List<PeriodDto>> GetRecordedPeriodsAsync();
   Task<List<string>> GetRecordedNumbersAsync();
   Task<List<string>> GetRecordedStationsAsync();
   Task<List<string>> GetRecordedSchemasAsync();
}