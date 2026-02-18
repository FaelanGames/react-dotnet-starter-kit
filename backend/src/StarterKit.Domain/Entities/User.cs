namespace StarterKit.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    public User(Guid id, string email, string passwordHash, DateTime createdUtc)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Email = NormalizeEmail(email);
        CreatedUtc = createdUtc == default ? DateTime.UtcNow : createdUtc;
        UpdatePasswordHash(passwordHash);
    }

    public static User CreateNew(string email, string passwordHash)
        => new(Guid.NewGuid(), email, passwordHash, DateTime.UtcNow);

    public void UpdatePasswordHash(string newHash)
    {
        PasswordHash = newHash ?? throw new ArgumentNullException(nameof(newHash));
    }

    private static string NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        return email.Trim().ToLowerInvariant();
    }
}
