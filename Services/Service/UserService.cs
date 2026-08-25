using DB.Repositories;
using Domain.Classes;
using Domain.DTO;

namespace Services;

public class UserService : IUserService
{
    private readonly IUserRepo _userRepo;
    
    public UserService(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task CreateUserAsync(AuthDto request)
    {
        string password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        User user = new User(request.Name, password);
        
        await _userRepo.CreateUserAsync(user);
    }

    public async Task<bool> CheckUserAsync(AuthDto request)
    {
        var user = await _userRepo.GetUserByUsernameAsync(request.Name);
        string password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        return password == user.PasswordHash;
    }
}