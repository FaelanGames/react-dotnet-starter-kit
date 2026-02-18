using StarterKit.Application.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StarterKit.Api.Tests;

public sealed class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidInput_ThenMeReturnsCurrentUser()
    {
        // Arrange
        var email = $"user{Guid.NewGuid():N}@example.com";
        var password = "Password123!";

        // Act: register
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));
        registerRes.EnsureSuccessStatusCode();

        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));

        // Act: call protected endpoint with token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var meRes = await _client.GetAsync("/api/users/me");
        meRes.EnsureSuccessStatusCode();

        var me = await meRes.Content.ReadFromJsonAsync<MeResponseDto>();
        Assert.NotNull(me);
        Assert.Equal(email.ToLowerInvariant(), me!.Email);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        // Ensure no token
        _client.DefaultRequestHeaders.Authorization = null;

        var res = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Register_SameEmailTwice_ReturnsConflict()
    {
        var email = $"dupe{Guid.NewGuid():N}@example.com";
        var password = "Password123!";

        var first = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));
        first.EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var message = await second.Content.ReadAsStringAsync();
        Assert.Contains("already registered", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"login{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));

        var res = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(email, "Wrong123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_RotatesTokens()
    {
        var email = $"refresh{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);

        var refreshRes = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(auth!.RefreshToken));
        refreshRes.EnsureSuccessStatusCode();
        var refreshed = await refreshRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(refreshed);
        Assert.NotEqual(auth.AccessToken, refreshed!.AccessToken);
        Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);

        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(auth.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var email = $"refreshinvalid{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);

        var tampered = auth!.RefreshToken + "invalid";
        var res = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(tampered));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);

        var followUp = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(auth.RefreshToken));
        followUp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Logout_WithProvidedToken_RevokesRefreshToken()
    {
        var email = $"logout{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto(email, password));
        registerRes.EnsureSuccessStatusCode();
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);

        var logoutRes = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshRequestDto(auth!.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, logoutRes.StatusCode);

        var refreshRes = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(auth.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refreshRes.StatusCode);
    }
}

