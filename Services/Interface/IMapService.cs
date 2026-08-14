using Domain.Classes;

namespace Services;

public interface IMapService
{
    Task UploadMapSchemaAsync(MapSchema schema);
}