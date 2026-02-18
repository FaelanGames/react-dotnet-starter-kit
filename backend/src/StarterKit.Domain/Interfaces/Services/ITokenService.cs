using StarterKit.Domain.Entities;

namespace StarterKit.Domain.Interfaces.Services;

// TODO: Move class and record to their own files.
public interface ITokenService
{
    string CreateAccessToken(User user);
    RefreshTokenResult CreateRefreshToken();
    int AccessTokenExpiresSeconds { get; }
}

public sealed record RefreshTokenResult(string Token, string Hash, DateTime ExpiresUtc);

public interface ITokenHashingService
{
    string Hash(string token);
}
