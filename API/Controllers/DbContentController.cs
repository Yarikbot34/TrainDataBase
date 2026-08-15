using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Route("api/v1/content")]
public class DbContentController : ControllerBase
{
    private readonly IdbContentRepo _contentRepo;

    public DbContentController(IdbContentRepo db)
    {
        _contentRepo = db;
    }


    [HttpGet("writedYears")]
    public async Task<IActionResult> GetWritedYearsAsync()
    {
        var answ = await _contentRepo.GetRecordedYearsAsync();
        return Ok(answ);
    }

    [HttpGet("writedMonths")]
    public async Task<IActionResult> GetWritedMonthsAsync()
    {
        var answ = await _contentRepo.GetRecordedMonthsAsync();
        return Ok(answ);
    }

    [HttpGet("writedNumbers")]
    public async Task<IActionResult> GetWritedNumbersAsync()
    {
        var answ = await _contentRepo.GetRecordedNumbersAsync();
        return Ok(answ);
    }

    [HttpGet("writedStations")]
    public async Task<IActionResult> GetWritedStationsAsync()
    {
        var answ = await _contentRepo.GetRecordedStationsAsync();
        return Ok(answ);
    }

    [HttpGet("writedSchemas")]
    public async Task<IActionResult> GetWritedSchemasAsync()
    {
        var answ = await _contentRepo.GetRecordedSchemasAsync();
        return Ok(answ);
    }
}