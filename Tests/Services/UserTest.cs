using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Moq;
using Services;

namespace Tests.Services;

public class UserTest
{
    private readonly IUserService _userService;
    private readonly Mock<IUserRepo> _userRepoMock;
    
    public UserTest()
    {
        _userRepoMock = new Mock<IUserRepo>();
        _userService = new UserService(_userRepoMock.Object);
    }

    [Fact]
    public async Task CreateUserTest()
    {
        var tstDto = new AuthDto()
        {
            Name = "test",
            Password = "test",
        };
        var tstRole = "View";
        User? tstUser = null;

        _userRepoMock.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(user => tstUser = user)
            .Returns(Task.CompletedTask);

        await _userService.CreateUserAsync(tstDto, tstRole);
        
        _userRepoMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
        
        Assert.NotNull(tstUser);
        Assert.Equal(tstUser.Username, tstDto.Name);
        Assert.NotEqual(tstDto.Password, tstUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(tstDto.Password, tstUser.PasswordHash));
    }
    
    [Fact]
    public async Task CreateUserTestWithWrongRole()
    {
        var tstDto = new AuthDto()
        {
            Name = "test",
            Password = "test",
        };
        var tstRole = "Manager";
        
        await Assert.ThrowsAsync<System.ArgumentException>(() => _userService.CreateUserAsync(tstDto, tstRole));
    }
}