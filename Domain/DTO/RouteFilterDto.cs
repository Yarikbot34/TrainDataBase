
namespace Domain.DTO;

public class RouteFilterDto
{
    public List<PeriodDto>? period { get; set; }
    public string? number { get; set; }
    public string? stationFrom { get; set; }
    public string? stationTo { get; set; }
}
