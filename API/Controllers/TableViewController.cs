using Microsoft.AspNetCore.Mvc;
using DB;
using DB.Repositories;
using Route = DB.Route;

namespace API.Controllers;


[ApiController]
[Route("api/v1/TableView")]
public class TableViewController : ControllerBase
{
    IRawDataRepo _rawDataRepo;
    

    public TableViewController(IRawDataRepo rawDataRepo)
    {
        _rawDataRepo = rawDataRepo;
    }
    
    [HttpGet("routes")]
    public async Task<ActionResult> GetRoutes()
    {
        var answer = await _rawDataRepo.getRoutesAsync();
        return Ok(answer);
    }
    [HttpGet("trains")]
    public async Task<ActionResult> GetTrains()
    {
        var answer = await _rawDataRepo.getTrainsAsync();
        return Ok(answer);
    }[HttpGet("stations")]
    public async Task<ActionResult> GetStations()
    {
        var answer = await _rawDataRepo.getStationsAsync();
        return Ok(answer);
    }
    
}