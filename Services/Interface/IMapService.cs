using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface IMapService
{
    Task UploadMapSchemaAsync(MapSchema schema);
    Task<MapSchemaDto> GetMapSchemaAsync(string schemaName, int[] years, int[] months);
}