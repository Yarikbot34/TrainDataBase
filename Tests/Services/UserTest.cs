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

    [Fact]
    public async Task CheckUserTest()
    {
        var tstTrueDto = new AuthDto()
        {
            Name = "test",
            Password = "test",
        };
        var tstFalseDto = new AuthDto()
        {
            Name = "testAdmin",
            Password = "test123",
        };
        var tstPassword = BCrypt.Net.BCrypt.HashPassword("test");
        _userRepoMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(new User()
            {
                Id = 1,
                Username = "test",
                PasswordHash = tstPassword
            });
        
        var answT = await _userService.CheckUserAsync(tstTrueDto);
        var answF = await _userService.CheckUserAsync(tstFalseDto);
        
        _userRepoMock.Verify(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()), Times.Exactly(2));
        
        Assert.True(answT);
        Assert.False(answF);
    }

    [Fact]
    public async Task CheckNullUserTest()
    {
        var tstDto = new AuthDto()
        {
            Name = "test",
            Password = "test",
        };
        _userRepoMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(null as User);
        
        await Assert.ThrowsAsync<Exception>(() => _userService.CheckUserAsync(tstDto));
    }
}