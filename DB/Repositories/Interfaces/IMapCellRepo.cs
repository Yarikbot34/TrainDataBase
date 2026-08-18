using Domain.Classes;

namespace DB.Repositories;

public interface IMapCellRepo
{
    Task<MapCell> GetCellById(int id);
    Task<List<MapCell>> GetCellsBySchemaId(int schemaId);
    Task RemoveCells(IEnumerable<MapCell> cells);
}