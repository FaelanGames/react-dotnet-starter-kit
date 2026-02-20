using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StarterKit.Api.Features.Users;
using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;

namespace StarterKit.Api.Tests;

public sealed class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>(MockBehavior.Strict);
    }

    [Fact]
    public async Task Me_WithMissingUidClaim_ReturnsUnauthorized()
    {
        var controller = CreateController(new ClaimsPrincipal(new ClaimsIdentity()));

        var result = await controller.Me();

        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockUserService.VerifyAll();
    }

    [Fact]
    public async Task Me_WithInvalidUidClaim_ReturnsUnauthorized()
    {
        var principal = BuildPrincipal(new Claim("uid", "not-a-guid"));
        var controller = CreateController(principal);

        var result = await controller.Me();

        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockUserService.VerifyAll();
    }

    [Fact]
    public async Task Me_WithValidUidClaim_ReturnsServiceResponse()
    {
        var userId = Guid.NewGuid();
        var expected = new MeResponseDto(userId, "user@example.com", DateTime.UtcNow);

        _mockUserService
            .Setup(s => s.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var principal = BuildPrincipal(new Claim("uid", userId.ToString()));
        var controller = CreateController(principal);

        var result = await controller.Me();

        Assert.Null(result.Result);
        Assert.Equal(expected, result.Value);
        _mockUserService.VerifyAll();
    }

    private UsersController CreateController(ClaimsPrincipal user)
    {
        return new UsersController(_mockUserService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    private static ClaimsPrincipal BuildPrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
