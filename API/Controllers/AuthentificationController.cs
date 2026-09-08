using Domain.Classes;
using Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Route("api/v1/authentification")]
public class AuthentificationController : ControllerBase
{
    private readonly IAuthService _authorizationService;
    
    public AuthentificationController(IAuthService authorizationService)
    {
        _authorizationService = authorizationService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(AuthDto user)
    {
        string tokenStr = await _authorizationService.LoginUserAsync(user);
        return Ok(new {token = tokenStr});
    }
    
    
}