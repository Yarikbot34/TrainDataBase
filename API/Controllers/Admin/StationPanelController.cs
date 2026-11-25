using System.ComponentModel.DataAnnotations;
using Domain.DTO;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;


[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/adminPanel")]
public class StationPanelController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationPanelController(IStationService stationService)
    {
        _stationService = stationService;
    }

    [HttpGet("stations")]
    public async Task<IActionResult> GetStationsAsync()
    {
        var answ = await _stationService.GetStationsAsync();
        return Ok(answ);
    }

    [HttpPost("stations/add")]
    public async Task<IActionResult> AddStationAsync([FromBody] StationDto stationDto)
    {
        try
        {
            await _stationService.WriteStationFromDtoAsync(stationDto);
            return Ok();
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("stations/{id}")]
    public async Task<IActionResult> DeleteStationAsync(int id)
    {
        try
        {
            await _stationService.DeleteStationByIdAsync(id);
            return Ok();
        }
        catch (Exception e)
        {
            return  BadRequest(e.Message);
        }
    }

    [HttpPut("stations/{id}")]
    public async Task<IActionResult> UpdateStationAsync(int id, [FromBody] StationDto stationDto)
    {
        try
        {
            var result = await _stationService.UpdateStationAsync(id, stationDto);
            return result switch
            {
                StationUpdateResult.Updated => Ok(),
                StationUpdateResult.NotFound => NotFound("Станция с таким id не найдена"),
                StationUpdateResult.NameConflict => Conflict("Станция с таким названием уже существует"),
                _ => throw new InvalidOperationException($"Неизвестный результат обновления станции: {result}")
            };
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

}