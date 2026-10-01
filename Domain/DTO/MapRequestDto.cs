namespace Domain.DTO;

public class MapRequestDto
{
    public List<PeriodDto> Periods { get; set; } = new();
    public string schemaName { get; set; }
}