using System.Text.Json.Serialization;

namespace Domain.Classes;

public class MapCell
{
    [JsonIgnore]
    public int Id { get; set; }
    public string SchemaId { get; set; }
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

public struct Data
{
    public int? Load {get; set;}
    public string? Label {get; set;}
}

public struct Coords
{
    public int X { get; set; }
    public int Y { get; set; }
}

public struct CellReference
{
    public string Cell { get; set; }
}

