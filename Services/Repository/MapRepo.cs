using DB;
using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class MapRepo : IMapRepo
{
    private readonly AppDbContext ldb;
    
    public  MapRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task UploadMapSchemaAsync(MapSchema schema)
    {
        var cells = schema.MapCells;
        var writedSchemas = ldb.MapSchemas.Select(s => s.Name).ToList();
        if (!writedSchemas.Contains(schema.Name))
        {
            GetStationsForCells(cells);
            ldb.MapSchemas.Add(schema);
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
            
            GetStationsForCells(schema.MapCells);
            
            foreach (var cell in schema.MapCells)
            {
                cell.Id = 0;
                cell.SchemaId = oldSchema.Id;
                oldSchema.MapCells.Add(cell);
            }
            
        }

        void GetStationsForCells(List<MapCell> mapCells)
        {
            mapCells = mapCells.OrderByDescending(c => c.Type == "node").ToList();
            foreach (var mapCell in mapCells)
            {
                if (mapCell.Type == "node")
                {
                    var stat = ldb.Stations.First(s => s.Name == mapCell.Data.Label);
                    mapCell.Station = stat;
                }
                else if (mapCell.Type == "edge")
                {
                    var statFrom = mapCells.First(c => c.CellId == mapCell.Source.Cell).Station;
                    var statTo = mapCells.First(c => c.CellId == mapCell.Target.Cell).Station;
                    mapCell.SourceStation = statFrom;
                    mapCell.TargetStation = statTo;
                }
            }
        }
        
        await ldb.SaveChangesAsync();
    }
}