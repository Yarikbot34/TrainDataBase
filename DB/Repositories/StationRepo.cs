using Domain.Classes;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DB.Repositories;

public class StationRepo : IStationRepo
{
    private readonly AppDbContext ldb;
    
    public StationRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task WriteStationAsync(Station station)
    {
        ldb.Stations.Add(station);
        await ldb.SaveChangesAsync();
    }

    public async Task WriteStationsAsync(IEnumerable<Station> stations)
    {
        ldb.Stations.AddRange(stations);
        await ldb.SaveChangesAsync();
    }
    
    public async Task<Station> GetStationByIdAsync(int stationId)
    {
        return await ldb.Stations.FindAsync(stationId);
    }

    public async Task<Station?> GetStationByNameAsync(string name)
    {
        var answ = await ldb.Stations
            .FirstOrDefaultAsync(s => s.Name == name);
        return answ;
    }

    public async Task<List<Station>> GetAllStationsAsync()
    {
        return await ldb.Stations.ToListAsync();
    }

    public async Task DeleteStationByIdAsync(int stationId)
    {
        var station = await ldb.Stations.FindAsync(stationId);
        if (station is null) throw new KeyNotFoundException("Станция с таким id не найдена");
        ldb.Stations.Remove(station);
        await ldb.SaveChangesAsync();
    }

    public async Task<StationUpdateResult> UpdateStationAsync(int stationId, string name, string? stationClass)
    {
        try
        {
            // Name — альтернативный ключ, который нельзя изменить через отслеживаемую сущность EF Core.
            var affectedRows = await ldb.Stations
                .Where(station => station.Id == stationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(station => station.Name, name)
                    .SetProperty(station => station.Class, stationClass));

            return affectedRows == 0 ? StationUpdateResult.NotFound : StationUpdateResult.Updated;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation &&
                                           ex.ConstraintName == "AK_Stations_Name")
        {
            return StationUpdateResult.NameConflict;
        }
    }
}