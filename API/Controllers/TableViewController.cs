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
    
}