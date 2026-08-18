using Domain.Classes;
namespace DB.Repositories;

public class MapCellRepo : IMapCellRepo
{
    private readonly AppDbContext ldb;

    public MapCellRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task<MapCell> GetCellById(int id)
    {
        var answ = await ldb.MapCells.FindAsync(id);
        return answ;
    }

    public async Task<List<MapCell>> GetCellsBySchemaId(int schemaId)
    {
        var answ = ldb.MapCells
            .Where(c => c.SchemaId == schemaId)
            .ToList();
        return answ;
    }
    
    public async Task RemoveCells(IEnumerable<MapCell> cell)
    {
        ldb.MapCells.RemoveRange(cell);
        await ldb.SaveChangesAsync();
    }
    
    
}