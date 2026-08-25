using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
public class AuthentificationController : ControllerBase
{

    [HttpGet("test")]
    public IActionResult test()
    {
        var amsw = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        return Ok(amsw);
    }
    
    
}