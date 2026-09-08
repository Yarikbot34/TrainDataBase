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
        try
        {
            var answ = await _AuthService.RegisterNewUserAsync(request, User);
            if (answ) return Ok(answ);
            return BadRequest();
        }
        catch  (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpDelete("Users/{id}")]
    public async Task<IActionResult> DeleteUserAsync(int id, [FromBody] string adminPassword)
    {
        try
        {
            bool answ = await _UserService.DeleteUserAsync(id, adminPassword, User);
            if (answ) return Ok();
            return BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}