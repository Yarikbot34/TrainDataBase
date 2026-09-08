using Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;


[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/adminPanel")]
public class UserPanelController : ControllerBase
{
    private readonly IUserService _UserService;
    private readonly IAuthService _AuthService;
    
    
    public UserPanelController(IUserService userService, IAuthService authService)
    {
        _UserService = userService;
        _AuthService = authService;
    }

    [HttpGet("Users")]
    public async Task<IActionResult> GetUsersAsync()
    {
        var answ = await _UserService.GetUsersAsync();
        return Ok(answ);
    }

    [HttpPost("Users/Add")]
    public async Task<IActionResult> AddUserAsync([FromBody] AddNewUserDto request)
    {
        var answ = await _AuthService.RegisterNewUserAsync(request, User);
        if (answ) return Ok(answ);
        return BadRequest();
        
    }
}