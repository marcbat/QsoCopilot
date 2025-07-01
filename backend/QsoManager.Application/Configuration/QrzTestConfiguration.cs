namespace QsoManager.Application.Configuration;

/// <summary>
/// Configuration QRZ pour les tests d'intégration uniquement
/// </summary>
public class QrzTestConfiguration
{
    public const string ConfigurationSection = "QrzTest";
    
    /// <summary>
    /// Username QRZ.com pour les tests (stocké dans les secrets)
    /// </summary>
    public string? TestUsername { get; set; }
    
    /// <summary>
    /// Password QRZ.com pour les tests (stocké dans les secrets)
    /// </summary>
    public string? TestPassword { get; set; }
    
    /// <summary>
    /// Indique si les credentials de test sont configurés
    /// </summary>
    public bool AreTestCredentialsConfigured => 
        !string.IsNullOrEmpty(TestUsername) && !string.IsNullOrEmpty(TestPassword);
}
