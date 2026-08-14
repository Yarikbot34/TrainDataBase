using System.Text.Json.Serialization;

namespace Domain.Classes;

public class MapSchema
{
    public int Id { get; set; } 
    public string Name { get; set; }
    public  string Description { get; set; }
    [JsonPropertyName("cells")]
    public List<MapCell> MapCells { get; set; }
}