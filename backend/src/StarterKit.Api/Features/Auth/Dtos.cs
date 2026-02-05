namespace StarterKit.Api.Features.Auth;

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds
);
