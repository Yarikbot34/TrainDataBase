using Microsoft.AspNetCore.Mvc;
using DB;
using DB.Repositories;
using Domain.Classes;
using Services;

namespace API.Controllers;


[ApiController]
[Route("api/v1/TableView")]
public class TableViewController : ControllerBase
{
    IRawDataRepo _rawDataRepo;
    ITrainRepo _trainRepo;
    

    public TableViewController(IRawDataRepo rawDataRepo,  ITrainRepo trainRepo)
    {
        _rawDataRepo = rawDataRepo;
        _trainRepo = trainRepo;
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
    }

    [HttpGet("trains/{year}/{month}/{number}")]
    public async Task<ActionResult> GetTrains(int year, int month, string number)
    {
        var answer = await _trainRepo.GetTrainsByPeriodAndNumber(year, month, number);
        return Ok(answer);
    }
    
    [HttpGet("stations")]
    public async Task<ActionResult> GetStations()
    {
        var answer = await _rawDataRepo.getStationsAsync();
        return Ok(answer);
    }
    
}