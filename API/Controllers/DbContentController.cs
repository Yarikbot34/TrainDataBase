using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Route("api/v1/content")]
public class DbContentController : ControllerBase
{
    private readonly IdbContentService _contentService;

    public DbContentController(IdbContentService cdb)
    {
        _contentService = cdb;
    }


    [HttpGet("writedYears")]
    public async Task<IActionResult> GetWritedYearsAsync()
    {
        var answ = await _contentService.GetRecordedYearsAsync();
        return Ok(answ);
    }

    [HttpGet("writedPeriods")]
    public async Task<IActionResult> GetWritedMonthsAsync()
    {
        var answ = await _contentService.GetRecordedPeriodsAsync();
        return Ok(answ);
    }

    [HttpGet("writedNumbers")]
    public async Task<IActionResult> GetWritedNumbersAsync()
    {
        var answ = await _contentService.GetRecordedNumbersAsync();
        return Ok(answ);
    }

    [HttpGet("writedStations")]
    public async Task<IActionResult> GetWritedStationsAsync()
    {
        var answ = await _contentService.GetRecordedStationsAsync();
        return Ok(answ);
    }

    [HttpGet("writedSchemas")]
    public async Task<IActionResult> GetWritedSchemasAsync()
    {
        var answ = await _contentService.GetRecordedSchemasAsync();
        return Ok(answ);
    }
    
    [HttpGet("WritedTransactionTypes")]
    public async Task<IActionResult> GetTransactionTypes()
    {
        var answ = await _contentService.GetTransactionTypesAsync();
        return Ok(answ);
    }
}