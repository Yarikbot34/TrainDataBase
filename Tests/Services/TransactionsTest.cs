using System.Security.Claims;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Services;

namespace Tests.Services;

public class TransactionsTest
{
    private readonly Mock<ITransactionRepo> _transactionRepoMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUserRepo> _userRepoMock;
    private readonly TransactionsService _transactionsService;

    public TransactionsTest()
    {
        _transactionRepoMock = new Mock<ITransactionRepo>();
        _userServiceMock = new Mock<IUserService>();
        _userRepoMock = new Mock<IUserRepo>();

        var services = new ServiceCollection();
        services.AddHybridCache();
        var cache = services.BuildServiceProvider().GetRequiredService<HybridCache>();

        _transactionsService = new TransactionsService(_transactionRepoMock.Object, _userServiceMock.Object,
            _userRepoMock.Object, cache);
    }

    [Fact]
    public async Task GetTransactionsList()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, Year = 2024, Month = 1, Description = "Добавление", UnitsGet = 10 },
            new() { Id = 2, Year = 2024, Month = 2, Description = "Обновление", UnitsGet = 5 }
        };
        var filter = new TransactionFilterDto { StartDate = new DateTime(2024, 1, 1) };
        _transactionRepoMock.Setup(repo => repo.GetTransactionsByFilterAsync(filter)).ReturnsAsync(transactions);

        var result = await _transactionsService.GetTransactionsListAsync(filter);

        Assert.Equal(2, result.Count);
        Assert.Equal(transactions[0].Description, result[0].Description);
        Assert.Equal(transactions[1].UnitsGet, result[1].UnitCount);
        _transactionRepoMock.Verify(repo => repo.GetTransactionsByFilterAsync(filter), Times.Once);
    }

    [Fact]
    public async Task PatchTransactionDescription()
    {
        var transaction = new Transaction { Id = 1, Description = "Старое описание" };
        var dto = new TransactionDto { Id = transaction.Id, Description = "Новое описание" };
        _transactionRepoMock.Setup(repo => repo.GetTransactionByIdAsync(transaction.Id)).ReturnsAsync(transaction);

        await _transactionsService.PatchTransactionDescFromDtoAsync(dto);

        Assert.Equal(dto.Description, transaction.Description);
        _transactionRepoMock.Verify(repo => repo.PathTransactionAsync(transaction), Times.Once);
    }

    [Fact]
    public async Task PatchTransactionDescriptionWhenTransactionNotFound()
    {
        var dto = new TransactionDto { Id = 1, Description = "Новое описание" };
        _transactionRepoMock.Setup(repo => repo.GetTransactionByIdAsync(dto.Id)).ReturnsAsync(null as Transaction);

        await Assert.ThrowsAsync<Exception>(() => _transactionsService.PatchTransactionDescFromDtoAsync(dto));

        _transactionRepoMock.Verify(repo => repo.PathTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUnitsByTransaction()
    {
        var transaction = new Transaction { Id = 1, Year = 2024, Month = 3, UnitsGet = 4 };
        var admin = new User { Id = 2, Username = "admin" };
        var dto = new TransactionDeleteDto { TransactionId = transaction.Id, AdminPassword = "password" };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, admin.Username) }));
        Transaction? deleteNote = null;

        _userServiceMock.Setup(service => service.CheckUserAsync(It.Is<AuthDto>(auth =>
            auth.Name == admin.Username && auth.Password == dto.AdminPassword))).ReturnsAsync(true);
        _transactionRepoMock.Setup(repo => repo.GetTransactionByIdAsync(transaction.Id)).ReturnsAsync(transaction);
        _userRepoMock.Setup(repo => repo.GetUserByUsernameAsync(admin.Username)).ReturnsAsync(admin);
        _transactionRepoMock.Setup(repo => repo.WriteNewTransactionAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(note => deleteNote = note).Returns(Task.CompletedTask);

        await _transactionsService.RemoveUnitsByTransactionAsync(dto, user);

        _transactionRepoMock.Verify(repo => repo.DeleteTransactionAsync(transaction), Times.Once);
        Assert.NotNull(deleteNote);
        Assert.Equal(Transaction.TransactionType.Delete, deleteNote.Type);
        Assert.Same(admin, deleteNote.User);
    }

    [Fact]
    public async Task RemoveUnitsByTransactionWithWrongPassword()
    {
        var dto = new TransactionDeleteDto { TransactionId = 1, AdminPassword = "wrong" };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }));
        _userServiceMock.Setup(service => service.CheckUserAsync(It.IsAny<AuthDto>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<Exception>(() => _transactionsService.RemoveUnitsByTransactionAsync(dto, user));

        _transactionRepoMock.Verify(repo => repo.GetTransactionByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUnitsByTransactionWithoutAuthentication()
    {
        var dto = new TransactionDeleteDto { TransactionId = 1, AdminPassword = "password" };

        await Assert.ThrowsAsync<System.Security.Authentication.AuthenticationException>(() =>
            _transactionsService.RemoveUnitsByTransactionAsync(dto, new ClaimsPrincipal()));

        _userServiceMock.Verify(service => service.CheckUserAsync(It.IsAny<AuthDto>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUnitsByTransactionWhenTransactionNotFound()
    {
        var dto = new TransactionDeleteDto { TransactionId = 1, AdminPassword = "password" };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }));
        _userServiceMock.Setup(service => service.CheckUserAsync(It.IsAny<AuthDto>())).ReturnsAsync(true);
        _transactionRepoMock.Setup(repo => repo.GetTransactionByIdAsync(dto.TransactionId))
            .ReturnsAsync(null as Transaction);

        await Assert.ThrowsAsync<Exception>(() => _transactionsService.RemoveUnitsByTransactionAsync(dto, user));

        _transactionRepoMock.Verify(repo => repo.DeleteTransactionAsync(It.IsAny<Transaction>()), Times.Never);
        _transactionRepoMock.Verify(repo => repo.WriteNewTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }
}
