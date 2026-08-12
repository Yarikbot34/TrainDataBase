
namespace Domain.DTO;

public class RouteFilterDto
{
    public int? year { get; set; }
    public int? month  { get; set; }
    public string? number { get; set; }
    public string? stationFrom { get; set; }
    public string? stationMiddle { get; set; }
    public string? stationTo { get; set; }
}
