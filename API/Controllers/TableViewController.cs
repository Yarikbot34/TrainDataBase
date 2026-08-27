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
        var answ = _routeService.GetRoutesByFilterWithSummAsync(filter);
        return Ok(answ);
    }
    
    [HttpGet("trains")]
    public async Task<ActionResult> GetTrains()
    {
        var answer = await _trainService.GetTrainsAsync();
        return Ok(answer);
    }

    [HttpGet("trains/{year}/{month}/{number}")]
    public async Task<ActionResult> GetTrains(int year, int month, string number)
    {
        string clearNumber = Uri.UnescapeDataString(number);
        var answer = await _trainService.GetTrainsByPeriodAndNumber(year, month, clearNumber);
        return Ok(answer);
    }
    
    [HttpGet("stations")]
    public async Task<ActionResult> GetStations()
    {
        var answer = await _stationService.GetStationsAsync();
        return Ok(answer);
    }
    
}