using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;


[ApiController]
[Route("api/v1/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly ISummaryDataService _summaryService;

    public StatisticsController(ISummaryDataService summaryService)
    {
        _summaryService = summaryService;
    }

    [HttpGet("payment/byYearInMonth/{year:int}")]
    public async Task<IActionResult> GetSummaryPaymentPerYearInMonth(int year)
    {
        int y = year > 100 ? DateTime.Now.Year % 100 : year;
        var answ = await _summaryService.GetYearPaymentDataInMonthAsync(y);
        return Ok(answ);
    }

    [HttpGet("passengers/byYearInMonth/{year:int}")]
    public async Task<IActionResult> GetPassengerPaymentPerYearInMonth(int year)
    {
        int y = year > 100 ? DateTime.Now.Year % 100 : year;
        var answ = await _summaryService.GetYearPassengerDataInMonthAsync(y);
        return Ok(answ);
    }
}