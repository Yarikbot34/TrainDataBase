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

    public async Task<List<MapCell>> GetMapSchemaAsync(string schemaName, int[] years, int[] months)
    {
        var schema = ldb.MapSchemas
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.Station)
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.TargetStation)
            .Include(sm=> sm.MapCells)
            .ThenInclude(mc => mc.SourceStation)
            .FirstOrDefault(s => s.Name == schemaName);
        
        var trains = ldb.Trains
            .Include(t => t.StationFrom)
            .Include(t => t.StationMiddle)
            .Include(t => t.StationTo)
            .Where(t => years.Contains(t.year) && months.Contains(t.month))
            .ToList();
            
            
        if (schema != null)
        {
            var cells = schema.MapCells;
            var nodesList = cells
                .Where(c => c.Station != null)
                .Select(c => c.Station)
                .ToList();
            Dictionary<Station, List<Station>> map = CreateConnectionMap(cells);
            foreach (var train in trains)
            {
                if (map.ContainsKey(train.StationFrom) && map.ContainsKey(train.StationTo) &&
                    train.StationMiddle == null)
                {
                    var way = GetWay(train.StationFrom, train.StationTo, map);
                    foreach (var cell in cells.Where(c => way.Contains(c.SourceStation) && way.Contains(c.TargetStation)))
                    {
                        cell.Data.Load += train.DayInRaise;
                    }
                }
                else if (map.ContainsKey(train.StationFrom) && map.ContainsKey(train.StationTo) &&
                         map.ContainsKey(train.StationMiddle))
                {
                    var way = GetWay(train.StationFrom, train.StationMiddle, map).ToHashSet();
                    way.ExceptWith(GetWay(train.StationMiddle, train.StationTo, map).ToHashSet());
                    cells.Where(c => way.Contains(c.SourceStation) && way.Contains(c.TargetStation))
                        .Select(c => c.Data.Load += 1);
                    
                }
            }
            return cells;
            
        }

        List<Station> GetWay(Station start, Station finish, Dictionary<Station, List<Station>> map)
        {
            if (start is null || finish is null)
                return null;
            
            var previous = new Dictionary<Station, Station>(); 
            var visited = new HashSet<Station> { start };
            var queue = new Queue<Station>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == finish)
                    break;

                if (!map.TryGetValue(current, out var neighbors))
                    continue;

                foreach (var next in neighbors)
                {
                    if (next is null) continue;

                    if (visited.Add(next))
                    {
                        previous[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }

            if (!visited.Contains(finish))
                return null;

            var path = new List<Station> { finish };
            while (path[^1] != start)
                path.Add(previous[path[^1]]);

            path.Reverse();
            return path;
        }
        

        Dictionary<Station, List<Station>> CreateConnectionMap(List<MapCell> cells)
        {
            Dictionary<Station, List<Station>> connectionsMap = new Dictionary<Station, List<Station>>();
            var edgesList = cells.Where(e => e.Type == "edge").ToList();
            var nodesList = cells.Where(n => n.Type == "node").ToList();
            foreach (var node in nodesList)
            {
                var connections =
                    edgesList.Where(e => e.SourceStation == node.Station || e.TargetStation == node.Station)
                        .Select(e => e.SourceStation == node.Station ? e.TargetStation : e.SourceStation)
                        .ToList();
                connectionsMap.Add(node.Station, connections);
            }
            return connectionsMap;
        }

        return null;
    }
}