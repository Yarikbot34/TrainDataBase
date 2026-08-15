namespace Services;

public interface IdbContentRepo
{
   Task<List<int>> GetRecordedYearsAsync();
   Task<List<int>> GetRecordedMonthsAsync();
   Task<List<string>> GetRecordedNumbersAsync();
   Task<List<string>> GetRecordedStationsAsync();
   Task<List<string>> GetRecordedSchemasAsync();
}