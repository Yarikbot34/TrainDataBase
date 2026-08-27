using Domain.Classes;

namespace DB.Repositories;

public interface IUserRepo
{
    Task CreateUserAsync(User user);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<List<User>> GetAllUsersAsync();

    bool CountOfUsersIsNull();
    bool UserExistsByUsername(string username);
}