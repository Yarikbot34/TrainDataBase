using Domain.Classes;
using Domain.DTO;
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


    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await ldb.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var answ = await ldb.Users.FirstOrDefaultAsync(u => u.Username == username);
        return answ;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await ldb.Users.ToListAsync();
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        ldb.Users.Update(user);
        await ldb.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserByIdAsync(int id)
    {
        if (id == 0) return false; // Админа не удалять
        
        var deleteUser = await ldb.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (deleteUser is null) return false;
        ldb.Users.Remove(deleteUser);
        await ldb.SaveChangesAsync();
        return true;
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