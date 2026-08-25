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
    
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(AuthDto user)
    {
        string tokenStr = await _authorizationService.RegisterUserAsync(user);
        return Ok(new {token = tokenStr});
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(AuthDto user)
    {
        string tokenStr = await _authorizationService.LoginUserAsync(user);
        return Ok(new {token = tokenStr});
    }
    
    
}