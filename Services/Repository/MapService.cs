using DB;
using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class MapService : IMapService
{
    private readonly AppDbContext ldb;
    
    public  MapService(AppDbContext db)
    {
        ldb = db;
    }

    public async Task UploadMapSchemaAsync(MapSchema schema)
    {
        var cells = schema.MapCells;
        var writedSchemas = ldb.MapSchemas.Select(s => s.Name).ToList();
        if (!writedSchemas.Contains(schema.Name))
        {
            ldb.MapSchemas.Add(schema);
            ldb.MapCells.AddRange(cells);
        }
        else
        {
            MapSchema? oldSchema = ldb.MapSchemas
                .Include(s => s.MapCells)
                .FirstOrDefault(s => s.Name == schema.Name);
            
            oldSchema.Description = schema.Description;

            var oldCells = oldSchema.MapCells.ToList();
            ldb.MapCells.RemoveRange(oldCells);
            oldSchema.MapCells.Clear();

            foreach (var cell in schema.MapCells ?? new List<MapCell>())
            {
                cell.Id = 0;
                cell.SchemaId = oldSchema.Id;
                oldSchema.MapCells.Add(cell);
            }
            ldb.SaveChanges();
            
        }
        
        

        
        
        ldb.MapSchemas.Add(schema);
        ldb.MapCells.AddRange(cells);
        await ldb.SaveChangesAsync();
    }
}