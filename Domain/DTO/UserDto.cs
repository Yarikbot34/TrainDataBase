using System.Text.Json.Serialization;
using Domain.Classes;

namespace Domain.DTO;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public DateTime DateCreated { get; set; }
    public string Role { get; set; }

    [JsonConstructor]
    public UserDto(){}

    public UserDto(User user)
    {
        Id = user.Id;
        Username = user.Username;
        DateCreated = user.Created;
        Role = user.Role;
    }
}