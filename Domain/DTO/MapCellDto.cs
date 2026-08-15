using System.Text.Json.Serialization;
using Domain.Classes;

namespace Domain.DTO;

public class MapCellDto
{
    public string id { get; set; }
    public string Shape { get; set; }
    
    public Coords? Position { get; set; }
    public CellReference? Source { get; set; }
    public CellReference? Target { get; set; }
    [JsonPropertyName("Data")]
    public Data CellData { get; set; }


    public MapCellDto(MapCell cell)
    {
        id = cell.CellId;
        Shape = cell.Shape;
        CellData = new Data()
        {
            Label = cell.Data.Label,
            trainLoad = 0,
            passengerLoad = 0,
            trains = new HashSet<string>()
        };
        if (Position != null)
        {
            Position = new Coords(){x = cell.Position.X,y = cell.Position.Y};
        }
        if (cell.Source != null && cell.Target != null)
        {
            Source = new CellReference() { Cell = cell.Source.Cell, Port = cell.Source.Port };
            Target = new CellReference() { Cell = cell.Target.Cell,  Port = cell.Target.Port };
        }
    }
    
}

public class Data
{
    public string? Label { get; set; }
    public int trainLoad { get; set; }
    public int passengerLoad { get; set; }
    public HashSet<string> trains { get; set; }
}

public class Coords
{
    public int x { get; set; }
    public int y { get; set; }
}

public class CellReference
{
    public string? Cell { get; set; }
    public string? Port { get; set; }
}