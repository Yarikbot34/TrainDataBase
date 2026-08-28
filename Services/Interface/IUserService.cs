using Domain.DTO;

namespace Services;

public interface IUserService
{
    Task CreateUserAsync(AuthDto request, string role);
    Task<bool> CheckUserAsync(AuthDto request);
}