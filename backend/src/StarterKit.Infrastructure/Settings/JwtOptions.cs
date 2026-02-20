namespace StarterKit.Infrastructure.Settings;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int ExpiresMinutes { get; init; } = 60;
    public int RefreshTokenDays { get; init; } = 14;
}
