using Domain.DTO;

namespace Services;

public interface ISummaryDataRepo
{
    Task<List<MonthDataDto>> GetYearDataAsync(int year); 
}