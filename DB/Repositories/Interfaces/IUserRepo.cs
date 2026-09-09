using System.Security.Claims;
using Domain.Classes;
using Domain.DTO;

namespace DB.Repositories;

public interface IUserRepo
{
    Task CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<List<User>> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserByIdAsync(int id);

    bool CountOfUsersIsNull();
    bool UserExistsByUsername(string username);
}