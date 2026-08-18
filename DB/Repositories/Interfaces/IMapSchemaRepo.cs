using Domain.Classes;

namespace DB.Repositories;

public interface IMapSchemaRepo
{
    Task WriteSchemaAsync(MapSchema mapSchema);
    Task<List<MapSchema>> GetAllSchemasAsync();
    Task<MapSchema> GetSchemaByIdAsync(int id);
    Task<MapSchema?> GetSchemaByNameAsync(string name);
}