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
}