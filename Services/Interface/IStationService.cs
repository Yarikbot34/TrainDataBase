using Domain.Classes;
using Domain.DTO;
using Domain.Enums;

namespace Services;

public interface IStationService
{
    Task<List<Station>> GetStationsAsync();
    Task WriteStationFromDtoAsync(StationDto dto);
    Task DeleteStationByIdAsync(int id);
    Task<StationUpdateResult> UpdateStationAsync(int id, StationDto dto);
}