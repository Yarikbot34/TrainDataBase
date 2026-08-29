namespace Domain.DTO;

public class TransactionFilterDto
{
    public List<string>? UserNames { get; set; }
    public List<PeriodDto>? Periods { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? TransactionTypes { get; set; }
}