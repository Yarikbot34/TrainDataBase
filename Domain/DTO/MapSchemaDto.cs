namespace Domain.DTO;

public class MapSchemaDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    
    public List<MapCellDto> data { get; set; }
}