namespace StarterKit.Api.Features.Users;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
