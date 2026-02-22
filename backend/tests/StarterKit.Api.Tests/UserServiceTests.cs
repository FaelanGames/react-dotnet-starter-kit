using Moq;
using StarterKit.Application.Results;
using StarterKit.Application.Services;
using StarterKit.Domain.Entities;
using StarterKit.Domain.Interfaces.Repositories;

namespace StarterKit.Api.Tests;

public sealed class UserServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserMissing_ReturnsUserNotFound()
    {
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = createService();

        var result = await service.GetCurrentUserAsync(Guid.NewGuid(), It.IsAny<CancellationToken>());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.UserNotFound, result.Error?.Code);
        _mockUserRepository.VerifyAll();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        var createdUtc = DateTime.UtcNow;
        var user = new User(userId, "USER@Example.com", "hash", createdUtc);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = createService();

        var response = await service.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value);
        Assert.Equal(userId, response.Value!.Id);
        Assert.Equal("user@example.com", response.Value.Email);
        Assert.Equal(createdUtc, response.Value.CreatedUtc);
        _mockUserRepository.VerifyAll();
    }

    private UserService createService() => new UserService(_mockUserRepository.Object);
}
