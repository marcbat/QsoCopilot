using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using QsoManager.Application.Interfaces.Services;
using System.Security.Cryptography;

namespace QsoManager.Infrastructure.Services;

/// <summary>
/// Service de hachage sécurisé des mots de passe utilisant PBKDF2
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // Générer un salt aléatoire
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hacher le mot de passe avec PBKDF2
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        // Combiner salt + hash et encoder en Base64
        byte[] combined = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, combined, 0, SaltSize);
        Array.Copy(hash, 0, combined, SaltSize, KeySize);

        return Convert.ToBase64String(combined);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
        
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword));

        try
        {
            // Décoder le hash stocké
            byte[] combined = Convert.FromBase64String(hashedPassword);
            
            if (combined.Length != SaltSize + KeySize)
                return false;

            // Extraire le salt
            byte[] salt = new byte[SaltSize];
            Array.Copy(combined, 0, salt, 0, SaltSize);

            // Extraire le hash stocké
            byte[] storedHash = new byte[KeySize];
            Array.Copy(combined, SaltSize, storedHash, 0, KeySize);

            // Hacher le mot de passe fourni avec le même salt
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Comparer les hashs de manière sécurisée (constant-time)
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
}
