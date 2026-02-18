namespace StarterKit.Application.Dtos;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds,
    DateTime RefreshTokenExpiresUtc
);
