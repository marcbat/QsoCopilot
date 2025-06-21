using Microsoft.Extensions.Configuration;
using QsoManager.Application.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace QsoManager.Infrastructure.Services;

/// <summary>
/// Service de chiffrement symétrique AES pour les données sensibles comme les mots de passe QRZ
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(IConfiguration configuration)
    {
        // Récupérer la clé de chiffrement depuis la configuration
        var keyString = configuration["Encryption:Key"];
        var ivString = configuration["Encryption:IV"];

        if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
        {
            throw new InvalidOperationException(
                "Les clés de chiffrement ne sont pas configurées. " +
                "Veuillez définir 'Encryption:Key' et 'Encryption:IV' dans la configuration.");
        }

        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);

        if (_key.Length != 32) // AES-256
        {
            throw new InvalidOperationException("La clé de chiffrement doit faire 32 bytes (256 bits) pour AES-256.");
        }

        if (_iv.Length != 16) // AES block size
        {
            throw new InvalidOperationException("L'IV doit faire 16 bytes pour AES.");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cryptoStream);

        writer.Write(plainText);
        writer.Flush();
        cryptoStream.FlushFinalBlock();

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        var encryptedBytes = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var memoryStream = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// Génère une nouvelle clé AES-256 encodée en base64 (pour la configuration)
    /// </summary>
    public static string GenerateKey()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        return Convert.ToBase64String(aes.Key);
    }

    /// <summary>
    /// Génère un nouvel IV AES encodé en base64 (pour la configuration)
    /// </summary>
    public static string GenerateIV()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        return Convert.ToBase64String(aes.IV);
    }
}
