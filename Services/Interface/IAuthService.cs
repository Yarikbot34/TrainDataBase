using Domain.DTO;

namespace Services;

public interface IAuthService
{
    Task<string> RegisterUserAsync(AuthDto user, string? role = "Basic");
    Task<string> LoginUserAsync(AuthDto user);
}