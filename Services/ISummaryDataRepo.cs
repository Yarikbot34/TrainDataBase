using Domain.DTO;

namespace Services;

public interface ISummaryDataRepo
{
    Task<List<MonthPaymentDataDto>> GetYearPaymentDataInMonthAsync(int year);
    Task<List<MonthPassengerDataDto>> GetYearPassengerDataInMonthAsync(int year);
    
}