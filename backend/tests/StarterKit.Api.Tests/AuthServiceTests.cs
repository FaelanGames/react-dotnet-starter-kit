using Moq;
using StarterKit.Application.Dtos;
using StarterKit.Application.Results;
using StarterKit.Application.Services;
using StarterKit.Domain.Entities;
using StarterKit.Domain.Interfaces.Repositories;
using StarterKit.Domain.Interfaces.Security;
using StarterKit.Domain.Interfaces.Services;

namespace StarterKit.Api.Tests;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUsers;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokens;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ITokenHashingService> _mockTokenHashing;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public AuthServiceTests()
    {
        _mockUsers = new Mock<IUserRepository>(MockBehavior.Strict);
        _mockRefreshTokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        _mockPasswordHasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        _mockTokenService = new Mock<ITokenService>(MockBehavior.Strict);
        _mockTokenHashing = new Mock<ITokenHashingService>(MockBehavior.Strict);
        _mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUsers
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRefreshTokens
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ReturnsEmailAlreadyRegistered()
    {
        _mockUsers
            .Setup(r => r.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new RegisterRequestDto("user@example.com", "Password123!");
        var service = CreateService();

        var result = await service.RegisterAsync(request, It.IsAny<CancellationToken>());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.EmailAlreadyRegistered, result.Error?.Code);

        _mockUsers.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRefreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_CreatesUserAndTokens()
    {
        var expectedExpiry = DateTime.UtcNow.AddDays(2);
        var registerRequest = new RegisterRequestDto("User@Example.com", "Password123!");
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var refreshtokeResult = new RefreshTokenResult(refreshToken, "refresh-hash", expectedExpiry);

        _mockUsers
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher
            .Setup(h => h.Hash(registerRequest.Password))
            .Returns("hashed-password");

        _mockTokenService
            .Setup(t => t.CreateRefreshToken())
            .Returns(refreshtokeResult);

        _mockTokenService
            .Setup(t => t.CreateAccessToken(It.IsAny<User>()))
            .Returns(accessToken);

        _mockTokenService
            .SetupGet(t => t.AccessTokenExpiresSeconds)
            .Returns(3600);

        var service = CreateService();

        var response = await service.RegisterAsync(registerRequest, It.IsAny<CancellationToken>());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value);
        Assert.Equal(accessToken, response.Value!.AccessToken);
        Assert.Equal(refreshToken, response.Value.RefreshToken);
        Assert.Equal(3600, response.Value.ExpiresInSeconds);

        _mockUsers.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Email == "user@example.com"), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRefreshTokens.Verify(
            r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUsers.VerifyAll();
        _mockRefreshTokens.VerifyAll();
        _mockPasswordHasher.VerifyAll();
        _mockTokenService.VerifyAll();
        _mockTokenHashing.VerifyAll();
        _mockUnitOfWork.VerifyAll();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsInvalidCredentials()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "stored-hash", DateTime.UtcNow);

        _mockUsers
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(h => h.Verify("Password123!", "stored-hash"))
            .Returns(false);

        var service = CreateService();
        var request = new LoginRequestDto("user@example.com", "Password123!");

        var response = await service.LoginAsync(request);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorCode.InvalidCredentials, response.Error?.Code);

        _mockRefreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_WithActiveToken_RotatesTokens()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "stored-hash", DateTime.UtcNow);
        var current = new RefreshToken(user.Id, "current-token-hash", DateTime.UtcNow.AddMinutes(30));

        _mockTokenHashing
            .Setup(h => h.Hash("refresh-token"))
            .Returns("current-token-hash");

        _mockRefreshTokens
            .Setup(r => r.GetByHashAsync("current-token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);

        _mockUsers
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockTokenService
            .Setup(t => t.CreateAccessToken(user))
            .Returns("new-access-token");

        _mockTokenService
            .Setup(t => t.CreateRefreshToken())
            .Returns(new RefreshTokenResult("new-refresh-token", "new-refresh-hash", DateTime.UtcNow.AddDays(1)));

        _mockTokenService
            .SetupGet(t => t.AccessTokenExpiresSeconds)
            .Returns(3600);

        var service = CreateService();

        var response = await service.RefreshAsync(new RefreshRequestDto("refresh-token"), It.IsAny<CancellationToken>());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value);
        Assert.Equal("new-access-token", response.Value!.AccessToken);
        Assert.Equal("new-refresh-token", response.Value.RefreshToken);
        Assert.Equal(3600, response.Value.ExpiresInSeconds);

        Assert.NotNull(current.RevokedUtc);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task LogoutAsync_WithEmptyToken_DoesNothing()
    {
        var service = CreateService();

        await service.LogoutAsync(new RefreshRequestDto(""), It.IsAny<CancellationToken>());

        _mockRefreshTokens.Verify(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private AuthService CreateService() => new AuthService(
        _mockUsers.Object,
        _mockRefreshTokens.Object,
        _mockPasswordHasher.Object,
        _mockTokenService.Object,
        _mockTokenHashing.Object,
        _mockUnitOfWork.Object);
}
