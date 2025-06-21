namespace QsoManager.Application.Interfaces.Services;

/// <summary>
/// Service pour hacher et vérifier les mots de passe
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hache un mot de passe
    /// </summary>
    /// <param name="password">Le mot de passe en clair</param>
    /// <returns>Le mot de passe haché</returns>
    string HashPassword(string password);

    /// <summary>
    /// Vérifie un mot de passe contre son hash
    /// </summary>
    /// <param name="password">Le mot de passe en clair</param>
    /// <param name="hashedPassword">Le mot de passe haché</param>
    /// <returns>True si le mot de passe correspond</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
