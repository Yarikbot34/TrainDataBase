namespace Domain.DTO;

public class WayConflictDto
{
    public int SchemaId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string trainNumber {get; set;}

    public List<List<string>> WayWariants { get; set; } = new List<List<string>>();
}