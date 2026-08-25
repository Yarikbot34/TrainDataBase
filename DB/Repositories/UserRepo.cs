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
}