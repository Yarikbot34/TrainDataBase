namespace Services;

public interface IdbContentService
{
   Task<List<int>> GetRecordedYearsAsync();
   Task<List<int>> GetRecordedMonthsAsync();
   Task<List<string>> GetRecordedNumbersAsync();
   Task<List<string>> GetRecordedStationsAsync();
   Task<List<string>> GetRecordedSchemasAsync();
}