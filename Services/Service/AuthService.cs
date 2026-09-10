using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using DB.Repositories;
using Domain.DTO;


namespace Services;

public class AuthService : IAuthService
{
    public static IReadOnlyCollection<string> Roles = new [] {"Admin", "View", "Upload"};
    private readonly IUserService _userService;
    private readonly IUserRepo _userRepo;

    public AuthService(IUserService userService, IUserRepo userRepo)
    {
        _userService = userService;
        _userRepo = userRepo;
    }

    public async Task<List<string>> GetRolesAsync()
    {
        return Roles.ToList();
    }
    
    public async Task<bool> RegisterNewUserAsync(AddNewUserDto request, ClaimsPrincipal user)
    {
        if (!Roles.Contains(request.Role)) throw new Exception("Ошибка, роль не найдена");
        if (user.Identity is null || 
            user.Identity.Name is null) throw new Exception("Ошибка аутентификации Админстратора");

        AuthDto testAdmin = new AuthDto
        {
            Name = user.Identity.Name,
            Password = request.Password,
        };
        if (await _userService.CheckUserAsync(testAdmin))
        {
            if (await _userRepo.GetUserByUsernameAsync(request.Name) is not null) 
                throw new Exception("Пользователь с таким именем уже есть в базе");

            AuthDto newUser = new AuthDto()
            {
                Name = request.Name,
                Password = request.Password,
            };
            string token = await RegisterUserAsync(newUser, request.Role);
            return String.IsNullOrEmpty(token);
        }
        Console.WriteLine($"Пользователь не прошел авторизацию\n {testAdmin.Name}\n{testAdmin.Password}");
        return false;
    }

    public async Task<string> LoginUserAsync(AuthDto user)
    {
        if (_userRepo.CountOfUsersIsNull() &&
            user.Name == Environment.GetEnvironmentVariable("ADMIN_NAME") &&
            user.Password == Environment.GetEnvironmentVariable("ADMIN_PASSWORD"))
        {
            return await RegisterUserAsync(user, "Admin");
        }

        var bdUser = await _userRepo.GetUserByUsernameAsync(user.Name);
        if (bdUser is not null && await _userService.CheckUserAsync(user))
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Ошибка генерации jwt ключа");

            var calims = new[]
            {
                new Claim(ClaimTypes.Name, bdUser.Username),
                new Claim(ClaimTypes.Role, bdUser.Role)
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

        throw new Exception("Пользователь не найден");
    }
    
    private async Task<string> RegisterUserAsync(AuthDto user, string? role = "View")
    {
        if (!Roles.Contains(role))
        {
            role = "View";
        }
        await _userService.CreateUserAsync(user, role);

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
}