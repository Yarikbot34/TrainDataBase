using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;


[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/adminPanel")]
public class UserPanelController : ControllerBase
{
    public readonly IUserService _UserService;
    
    public UserPanelController(IUserService userService)
    {
        _UserService = userService;
    }

    [HttpGet("Users")]
    public async Task<IActionResult> GetUsersAsync()
    {
        var answ = await _UserService.GetUsersAsync();
        return Ok(answ);
    }
}