using Domain.DTO;

namespace Services;

public interface IUserService
{
    Task CreateUserAsync(AuthDto request);
    Task<bool> CheckUserAsync(AuthDto request);
}