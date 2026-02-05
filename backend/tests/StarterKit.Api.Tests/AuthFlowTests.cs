using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterKit.Api.Features.Auth;
using StarterKit.Api.Features.Users;
using Xunit;

namespace StarterKit.Api.Tests;

public sealed class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_then_me_returns_current_user()
    {
        // Arrange
        var email = $"user{Guid.NewGuid():N}@example.com";
        var password = "Password123!";

        // Act: register
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password));
        registerRes.EnsureSuccessStatusCode();

        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));

        // Act: call protected endpoint with token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var meRes = await _client.GetAsync("/api/users/me");
        meRes.EnsureSuccessStatusCode();

        var me = await meRes.Content.ReadFromJsonAsync<MeResponse>();
        Assert.NotNull(me);
        Assert.Equal(email.ToLowerInvariant(), me!.Email);
    }

    [Fact]
    public async Task Me_without_token_is_unauthorized()
    {
        // Ensure no token
        _client.DefaultRequestHeaders.Authorization = null;

        var res = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
