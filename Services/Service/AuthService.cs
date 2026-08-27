using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using DB.Repositories;
using Domain.DTO;


namespace Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IUserRepo _userRepo;

    public AuthService(IUserService userService, IUserRepo userRepo)
    {
        _userService = userService;
        _userRepo = userRepo;
    }

    public async Task<string> RegisterUserAsync(AuthDto user, string? role = "Basic")
    {
        await _userService.CreateUserAsync(user);

        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Ошибка генерации jwt ключа");

        var calims = new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var cerds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TrDB-sys",
            audience: "TrDB-usr",
            claims: calims,
            expires: DateTime.Now.AddHours(12),
            signingCredentials: cerds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> LoginUserAsync(AuthDto user)
    {
        if (_userRepo.CountOfUsersIsNull())
        {
            return await RegisterUserAsync(user, "Admin");
        }

        var bdUser = await _userRepo.GetUserByUsernameAsync(user.Name);
        if (bdUser is not null && await _userService.CheckUserAsync(user))
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Ошибка генерации jwt ключа");

            var calims = new[] { new Claim(ClaimTypes.Name, user.Name) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var cerds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "TrDB-sys",
                audience: "TrDB-usr",
                claims: calims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: cerds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        throw new Exception("Пользователь не найден");
    }
}