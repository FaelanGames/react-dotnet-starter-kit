namespace StarterKit.Api.Features.Auth;

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds,
    DateTime RefreshTokenExpiresUtc
);

public sealed record RefreshRequest(string RefreshToken);
