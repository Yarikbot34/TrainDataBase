using Domain.Enums;

namespace Domain.Classes;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }
    public DateTime Created { get; }

    public User(string username, string passwordHash, string? role = "View")
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = (Role)Enum.Parse(typeof(Role), role);
        Created = DateTime.Now;
    }
    
    public User(){}
}