using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StarterKit.Api.Features.Auth;
using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;
using StarterKit.Application.Results;

namespace StarterKit.Api.Tests;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>(MockBehavior.Strict);
    }

    [Fact]
    public async Task Register_ReturnsOk_WithAuthResponse()
    {
        var request = new RegisterRequestDto("user@example.com", "Password123!");
        var response = new AuthResponseDto("access", "refresh", "Bearer", 3600, DateTime.UtcNow.AddDays(1));

        _mockAuthService
            .Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponseDto>.Success(response));

        var controller = CreateController();

        var result = await controller.Register(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(response, ok.Value);
        _mockAuthService.VerifyAll();
    }

    [Fact]
    public async Task Login_ReturnsOk_WithAuthResponse()
    {
        var request = new LoginRequestDto("user@example.com", "Password123!");
        var response = new AuthResponseDto("access", "refresh", "Bearer", 3600, DateTime.UtcNow.AddDays(1));

        _mockAuthService
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponseDto>.Success(response));

        var controller = CreateController();

        var result = await controller.Login(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(response, ok.Value);
        _mockAuthService.VerifyAll();
    }

    [Fact]
    public async Task Refresh_ReturnsOk_WithAuthResponse()
    {
        var request = new RefreshRequestDto("refresh-token");
        var response = new AuthResponseDto("access", "next-refresh", "Bearer", 3600, DateTime.UtcNow.AddDays(1));

        _mockAuthService
            .Setup(s => s.RefreshAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuthResponseDto>.Success(response));

        var controller = CreateController();

        var result = await controller.Refresh(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(response, ok.Value);
        _mockAuthService.VerifyAll();
    }

    [Fact]
    public async Task Logout_ReturnsNoContent_AndCallsService()
    {
        var request = new RefreshRequestDto("refresh-token");

        _mockAuthService
            .Setup(s => s.LogoutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = CreateController();

        var result = await controller.Logout(request);

        Assert.IsType<NoContentResult>(result);
        _mockAuthService.VerifyAll();
    }

    private AuthController CreateController()
    {
        return new AuthController(_mockAuthService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
