namespace Domain.Classes;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Class { get; set; }
    
    public bool InUse { get; set; }
    public bool IsOnMap { get; set; }
}