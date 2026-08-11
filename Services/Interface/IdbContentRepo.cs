namespace Services;

public interface IdbContentRepo
{
   Task<List<int>> GetRecordedYearsAsync();
}