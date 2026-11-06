using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using DB;
using Domain.Classes;
using Domain.DTO;
using Services;

namespace API.Controllers;


[ApiController]
[Route("api/v1/TableView")]
public class TableViewController : ControllerBase
{
    private readonly IStationService _stationService;
    private readonly ITrainService _trainService;
    private readonly IRouteService _routeService;

    public TableViewController(ITrainService trainService, IRouteService routeService, IStationService stationService)
    {
        _stationService = stationService;
        _trainService = trainService;
        _routeService = routeService;
    }
    
    [HttpGet("routes/{years?}")]
    public async Task<ActionResult> GetRoutes(List<int>? years = null)
    {
        if (years is null ) years = new List<int>{DateTime.Now.Year % 1000};
        var answer = await _routeService.GetRoutesWithSummAsync(years);
        return Ok(answer);
    }
    
    [HttpPost("routes/filter")]
    public async Task<ActionResult> GetRoutesFilter(RouteFilterDto filter)
    {
        var answ = await _routeService.GetRoutesByFilterWithSummAsync(filter);
        return Ok(answ);
    }
    
    [HttpGet("trains")]
    public async Task<ActionResult> GetTrains()
    {
        var answer = await _trainService.GetTrainsAsync();
        return Ok(answer);
    }

    [HttpPost("trains/filter")]
    public async Task<ActionResult> GetTrainsFilter(TrainFilterDto filter)
    {
        var answer = await _trainService.GetTrainsByFilterAsync(filter);
        return Ok(answer);
    }

    [HttpGet("trains/sorted")]
    public async Task<ActionResult> GetTrainsSorted()
    {
        var answer = await _trainService.GetTrainsSortedByNumberAndDescriptionAsync();
        return Ok(answer);
    }

    [HttpGet("trains/{year}/{month}/{number}")]
    public async Task<ActionResult> GetTrains(int year, int month, string number)
    {
        string clearNumber = Uri.UnescapeDataString(number);
        var answer = await _trainService.GetTrainsByPeriodAndNumber(year, month, clearNumber);
        return Ok(answer);
    }

    [HttpGet("routes/{year}/{month}/{number}")]
    public async Task<IActionResult> GetRoute(int year, int month, string number)
    {
        string clearNumber = Uri.UnescapeDataString(number);
        try
        {
            var answer = await _routeService.GetRouteByPeriodAndNumberAsync(year, month, clearNumber);
            return Ok(answer);
        } 
        catch (KeyNotFoundException ex)
        {
            return NotFound();
        } 
        catch  (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    
    [HttpGet("stations")]
    public async Task<ActionResult> GetStations()
    {
        var answer = await _stationService.GetStationsAsync();
        return Ok(answer);
    }
    
}
