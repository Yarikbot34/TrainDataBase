namespace Domain.DTO;

public class SchemaCheckDto
{
    public string SchemaName {get; set;}
    public List<PeriodDto> Periods {get; set;}
}