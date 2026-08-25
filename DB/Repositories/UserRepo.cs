using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace DB.Repositories;

public class UserRepo : IUserRepo
{
    private readonly AppDbContext ldb;
    
    public UserRepo(AppDbContext db)
    {
        ldb = db;
    }

    public async Task CreateUserAsync(User user)
    {
        await ldb.Users.AddAsync(user);
        await ldb.SaveChangesAsync();
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var answ = await ldb.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (answ is null)
        {
            throw new Exception("Пользователь под таким именем не найден");
        }
        return answ;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await ldb.Users.ToListAsync();
    }
    
    public bool CountOfUsersIsNull()
    {
        return ldb.Users.ToList().Count == 0;
    }

    public bool UserExistsByUsername(string username)
    {
        return ldb.Users.Any(u => u.Username == username);
    }
}