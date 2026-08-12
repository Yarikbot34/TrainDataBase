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
    private readonly IRawDataRepo _rawDataRepo;
    private readonly ITrainRepo _trainRepo;
    private readonly IRouteRepo _routeRepo;

    public TableViewController(IRawDataRepo rawDataRepo,  ITrainRepo trainRepo, IRouteRepo routeRepo)
    {
        _rawDataRepo = rawDataRepo;
        _trainRepo = trainRepo;
        _routeRepo = routeRepo;
    }
    
    [HttpGet("routes/{years?}")]
    public async Task<ActionResult> GetRoutes(int[]? years = null)
    {
        if (years == null ) years = new []{DateTime.Now.Year % 1000};
        var answer = await _rawDataRepo.getRoutesAsync(years);
        return Ok(answer);
    }

    [HttpGet("routes/filter")]
    public async Task<ActionResult> GetRoutesFilter([FromQuery] RouteFilterDto filter)
    {
        var answ = _routeRepo.GetRoutesByFilter(filter);
        return Ok(answ);
    }
    
    [HttpGet("trains")]
    public async Task<ActionResult> GetTrains()
    {
        var answer = await _rawDataRepo.getTrainsAsync();
        return Ok(answer);
    }

    [HttpGet("trains/{year}/{month}/{number}")]
    public async Task<ActionResult> GetTrains(int year, int month, string number)
    {
        string clearNumber = Uri.UnescapeDataString(number);
        var answer = await _trainRepo.GetTrainsByPeriodAndNumber(year, month, clearNumber);
        return Ok(answer);
    }
    
    [HttpGet("stations")]
    public async Task<ActionResult> GetStations()
    {
        var answer = await _rawDataRepo.getStationsAsync();
        return Ok(answer);
    }
    
}