using StarterKit.Infrastructure.Security;

namespace StarterKit.Api.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hasher = new PasswordHasher();

        var hash = hasher.Hash("Password123!");
        var verified = hasher.Verify("Password123!", hash);

        Assert.True(verified);
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        var verified = hasher.Verify("WrongPassword123!", hash);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_WithInvalidHashFormat_ReturnsFalse()
    {
        var hasher = new PasswordHasher();

        var verified = hasher.Verify("Password123!", "invalid-format");

        Assert.False(verified);
    }
}
