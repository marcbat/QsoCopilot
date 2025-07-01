namespace QsoManager.Application.Interfaces.Services;

/// <summary>
/// Service pour chiffrer et déchiffrer des données sensibles de manière réversible
/// Utilisé pour les mots de passe QRZ qui doivent pouvoir être récupérés pour renouveler les sessions
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Chiffre une chaîne de caractères
    /// </summary>
    /// <param name="plainText">Le texte en clair à chiffrer</param>
    /// <returns>Le texte chiffré encodé en base64</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Déchiffre une chaîne de caractères
    /// </summary>
    /// <param name="encryptedText">Le texte chiffré encodé en base64</param>
    /// <returns>Le texte en clair</returns>
    string Decrypt(string encryptedText);
}
