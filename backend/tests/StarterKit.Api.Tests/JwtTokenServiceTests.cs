using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using StarterKit.Domain.Entities;
using StarterKit.Infrastructure.Security;
using StarterKit.Infrastructure.Settings;

namespace StarterKit.Api.Tests;

public sealed class JwtTokenServiceTests
{

    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _service = CreateService();
    }

    [Fact]
    public void CreateAccessToken_ContainsExpectedClaims()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "hash", DateTime.UtcNow);

        var token = _service.CreateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("StarterKit", jwt.Issuer);
        Assert.Equal("StarterKit", Assert.Single(jwt.Audiences));
        Assert.True(jwt.ValidTo > DateTime.UtcNow);

        Assert.Contains(jwt.Claims,
            c => c.Type == "uid" && c.Value == user.Id.ToString());

        Assert.Contains(jwt.Claims,
            c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact]
    public void CreateRefreshToken_ReturnsHashMatchingToken()
    {
        var refresh = _service.CreateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(refresh.Token));
        Assert.False(string.IsNullOrWhiteSpace(refresh.Hash));

        var expectedHash = _service.Hash(refresh.Token);

        Assert.Equal(expectedHash, refresh.Hash);
        Assert.True(refresh.ExpiresUtc > DateTime.UtcNow.AddDays(13));
    }

    [Fact]
    public void Hash_IsDeterministic()
    {
        var service = CreateService();

        var first = service.Hash("token");
        var second = service.Hash("token");

        Assert.Equal(first, second);
    }

    private static JwtTokenService CreateService()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "StarterKit",
            Audience = "StarterKit",
            SigningKey = "this-is-a-long-signing-key-at-least-32-characters",
            ExpiresMinutes = 60,
            RefreshTokenDays = 14
        });

        return new JwtTokenService(options);
    }
}
