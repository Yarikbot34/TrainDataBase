using System.Text.Json.Serialization;

namespace Domain.DTO;

public class EditUserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    
    public string AdminPassword { get; set; }


    public UserDto GetUserDto()
    {
        return new UserDto
        {
            Id = Id,
            Username = Username,
            Role = Role,
        };
    }
}