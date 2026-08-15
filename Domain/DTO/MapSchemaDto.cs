using Domain.Classes;

namespace Domain.DTO;

public class MapSchemaDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    
    public List<MapCellDto> data { get; set; }


    public MapSchemaDto(MapSchema mapSchema, List<MapCellDto> data)
    {
        Name = mapSchema.Name;
        Description = mapSchema.Description;
        this.data = data;
    }
}