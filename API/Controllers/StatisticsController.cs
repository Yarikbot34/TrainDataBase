using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;


[ApiController]
[Route("api/v1/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly ISummaryDataRepo _summaryRepo;

    public StatisticsController(ISummaryDataRepo SummaryRepo)
    {
        _summaryRepo = SummaryRepo;
    }

    [HttpGet("payment/byYearInMonth/{year:int}")]
    public async Task<IActionResult> GetSummaryPaymentPerYearInMonth(int year)
    {
        int y = year > 100 ? DateTime.Now.Year % 100 : year;
        var answ = await _summaryRepo.GetYearPaymentDataInMonthAsync(y);
        return Ok(answ);
    }
}