using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class MapSchemaRepo : IMapSchemaRepo
{
    private readonly AppDbContext ldb;

    public MapSchemaRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task WriteSchemaAsync(MapSchema schema)
    {
        ldb.MapSchemas.Add(schema);
        await ldb.SaveChangesAsync();
    }

    public async Task<List<MapSchema>> GetAllSchemasAsync()
    {
        return await ldb.MapSchemas
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MapSchema> GetSchemaByIdAsync(int id)
    {
        var answ = await ldb.MapSchemas
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.Station)
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.TargetStation)
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.SourceStation)
            .FirstOrDefaultAsync(s => s.Id == id);
        return answ;
    }

    public async Task<MapSchema?> GetSchemaByNameAsync(string name)
    {
        var answ = await ldb.MapSchemas
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.Station)
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.TargetStation)
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.SourceStation)
            .FirstOrDefaultAsync(s => s.Name == name);
        return answ;
    }
}