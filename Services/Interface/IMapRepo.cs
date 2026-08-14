using Domain.Classes;

namespace Services;

public interface IMapRepo
{
    Task UploadMapSchemaAsync(MapSchema schema);
    Task<List<MapCell>> GetMapSchemaAsync(string schemaName, int[] years, int[] months);
}