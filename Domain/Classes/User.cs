namespace Domain.Classes;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public DateTime Created { get; }

    public User(string username, string passwordHash, string? role = "View")
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        Created = DateTime.Now;
    }
}