using System.Security.Claims;
using Domain.DTO;

namespace Services;

public interface IAuthService
{
    Task<List<string>> GetRolesAsync();
    Task<bool> RegisterNewUserAsync(AddNewUserDto request, ClaimsPrincipal user);
    Task<string> LoginUserAsync(AuthDto user);
}