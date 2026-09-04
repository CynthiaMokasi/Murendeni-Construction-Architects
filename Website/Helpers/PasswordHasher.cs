using System.Security.Cryptography;

namespace MurendeniConstructionArchitects.Helpers;

// Turns a plain-text password into something safe to store in the database,
// and checks a plain-text password against a stored hash at login time.
// We NEVER store the actual password anywhere - only this scrambled version.
public static class PasswordHasher
{
    private const int SaltSize = 16;     // random bytes mixed into the password before hashing
    private const int HashSize = 32;     // length of the resulting hash
    private const int Iterations = 100_000; // higher = slower to brute-force guess

    public static string HashPassword(string password)
    {
        // A random "salt" means two people with the same password get different hashes.
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        // Store the salt and hash together (separated by a dot) so we can verify later.
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        // FixedTimeEquals avoids leaking timing information about how much of the hash matched.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}