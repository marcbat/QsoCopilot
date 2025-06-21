using System;

namespace QsoManager.Infrastructure.Services.QRZ.Models;

/// <summary>
/// Modèle pour stocker les informations de session QRZ en cache
/// </summary>
public class QrzSessionCache
{
    /// <summary>
    /// Clé de session QRZ
    /// </summary>
    public string SessionKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Date d'expiration de la session
    /// </summary>
    public DateTime ExpirationDate { get; set; }
    
    /// <summary>
    /// Nom d'utilisateur associé à cette session
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Détermine si la session est encore valide
    /// </summary>
    public bool IsValid => DateTime.UtcNow < ExpirationDate;
    
    /// <summary>
    /// Détermine si la session expire dans les prochaines minutes (pour renouvellement proactif)
    /// </summary>
    /// <param name="minutesBeforeExpiration">Nombre de minutes avant expiration pour considérer un renouvellement</param>
    /// <returns>True si la session expire bientôt</returns>
    public bool IsExpiringSoon(int minutesBeforeExpiration = 30)
    {
        return DateTime.UtcNow.AddMinutes(minutesBeforeExpiration) >= ExpirationDate;
    }
}
