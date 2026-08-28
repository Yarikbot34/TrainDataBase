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

    public async Task CreateUserAsync(AuthDto request, string role)
    {
        string password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        User user = new User(request.Name, password, role);
        
        await _userRepo.CreateUserAsync(user);
    }

    public async Task<bool> CheckUserAsync(AuthDto request)
    {
        var user = await _userRepo.GetUserByUsernameAsync(request.Name);
        if (user is null) throw new Exception("Пользователь не найден");
        bool answ = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        
        return answ;
    }
}