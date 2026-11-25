using System.ComponentModel.DataAnnotations;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Domain.Enums;

namespace Services;

public class StationService : IStationService
{
    private readonly IStationRepo _stationRepo;

    public StationService(IStationRepo stationRepo)
    {
        _stationRepo = stationRepo;
    }
    
    
    public async Task<List<Station>> GetStationsAsync()
    {
        return await _stationRepo.GetAllStationsAsync();
    }

    public async Task WriteStationFromDtoAsync(StationDto dto)
    {
        var station = dto.ToStation();
        await _stationRepo.WriteStationAsync(station);
    }

    public async Task DeleteStationByIdAsync(int id)
    {
        await _stationRepo.DeleteStationByIdAsync(id);
    }

    public async Task<StationUpdateResult> UpdateStationAsync(int id, StationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 200)
            throw new ValidationException("Название станции должно быть непустым и не длиннее 200 символов.");

        if (dto.Class is { Length: > 50 })
            throw new ValidationException("Класс станции не должен быть длиннее 50 символов.");

        return await _stationRepo.UpdateStationAsync(id, dto.Name, dto.Class);
    }
}