namespace StarterKit.Api.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string SigningKey { get; init; } = "";
    public int ExpiresMinutes { get; init; } = 60;
    public int RefreshTokenDays { get; init; } = 14;
}
