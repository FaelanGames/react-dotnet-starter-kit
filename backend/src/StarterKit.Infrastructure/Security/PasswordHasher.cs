using System.Security.Cryptography;
using StarterKit.Domain.Interfaces.Security;

namespace StarterKit.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password, 
            salt, 
            Iterations, 
            HashAlgorithmName.SHA256, 
            KeySize
        );

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);

        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out var iterations)) return false;

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedKey = Convert.FromBase64String(parts[2]);

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password, 
                salt, 
                iterations, 
                HashAlgorithmName.SHA256, 
                expectedKey.Length
            );

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch
        {
            return false;
        }
    }
}
