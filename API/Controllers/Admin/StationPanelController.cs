using DB.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;


[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/adminPanel")]
public class StationPanelController : ControllerBase
{
    private readonly IStationRepo _stationRepo;

    public StationPanelController(IStationRepo stationRepo)
    {
        _stationRepo = stationRepo;
    }

    [HttpGet("stations")]
    public async Task<IActionResult> GetStationsAsync()
    {
        var answ = await _stationRepo.GetAllStationsAsync();
        return Ok(answ);
    }
    
}