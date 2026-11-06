namespace Domain.DTO;

public class TrainFilterDto
{
    public List<PeriodDto>? period { get; set; }
    public List<string> numbers { get; set; }
    public List<string> StationsFrom { get; set; }
    public List<string> StationsTo { get; set; }
    
    public bool IsEmpty => period is null || 
                           numbers.Count == 0 ||
                           StationsFrom.Count == 0 ||
                           StationsTo.Count == 0;
}