namespace StarterKit.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresUtc { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime? RevokedUtc { get; private set; }

    //public User? User { get; set; }

    public bool IsActive => RevokedUtc is null && DateTime.UtcNow <= ExpiresUtc;

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash ?? throw new ArgumentNullException(nameof(tokenHash));
        ExpiresUtc = expiresUtc;
        CreatedUtc = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (RevokedUtc is null)
        {
            RevokedUtc = DateTime.UtcNow;
        }
    }
}
