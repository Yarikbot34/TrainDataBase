using Domain.Classes;

namespace DB.Repositories;

public interface IUserRepo
{
    Task CreateUserAsync(User user);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<List<User>> GetAllUsersAsync();
    Task<bool> DeleteUserByIdAsync(int id);

    bool CountOfUsersIsNull();
    bool UserExistsByUsername(string username);
}