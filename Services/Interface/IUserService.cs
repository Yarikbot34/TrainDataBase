using System.Security.Claims;
using Domain.DTO;

namespace Services;

public interface IUserService
{
    Task CreateUserAsync(AuthDto request, string role);
    Task<bool> CheckUserAsync(AuthDto request);
    Task<List<UserDto>> GetUsersAsync();
    Task<bool> DeleteUserAsync(int id, string adminPassword, ClaimsPrincipal user);
}