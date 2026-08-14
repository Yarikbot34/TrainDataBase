using System.Text.Json.Serialization;

namespace Domain.Classes;

public class MapCell
{
    [JsonIgnore]
    public int Id { get; set; }
    [JsonIgnore]
    public int SchemaId { get; set; }
    [JsonIgnore]
    public MapSchema? Schema { get; set; }
    
    [JsonIgnore]
    public int? StationId { get; set; }
    [JsonIgnore]
    public Station? Station { get; set; }
    
    [JsonPropertyName("id")]
    public string CellId { get; set; }
    public string Shape { get; set; }
    
    public Data Data { get; set; }
    
    public Coords? Position { get; set; }
        
    public CellReference? Source { get; set; }
    public CellReference? Target { get; set; }

    [JsonIgnore]
    public string Type
    { get { return Position != null ? "node" : "edge"; } }

}

public class Data
{
    public int? Load {get; set;}
    public string? Label {get; set;}
}

public class Coords
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class CellReference
{
    public string Cell { get; set; }
}

