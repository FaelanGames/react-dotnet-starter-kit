using StarterKit.Api.Features.Users;

namespace StarterKit.Api.Auth;

public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTime ExpiresUtc { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedUtc { get; set; }

    public User? User { get; set; }

    public bool IsActive => RevokedUtc is null && DateTime.UtcNow <= ExpiresUtc;
}
