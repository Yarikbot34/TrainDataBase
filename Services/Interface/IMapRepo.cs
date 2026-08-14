using Domain.Classes;

namespace Services;

public interface IMapRepo
{
    Task UploadMapSchemaAsync(MapSchema schema);
}