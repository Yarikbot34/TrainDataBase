
namespace Domain.DTO;

public class RouteFilterDto
{
    public List<PeriodDto>? period { get; set; }
    public string? number { get; set; }
    public string? stationFrom { get; set; }
    public string? stationTo { get; set; }
    
    public bool IsEmpty => period is null || number is null || stationFrom is null || stationTo is null;
}
