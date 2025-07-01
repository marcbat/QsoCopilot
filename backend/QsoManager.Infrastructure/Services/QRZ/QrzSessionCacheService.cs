using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using QsoManager.Infrastructure.Services.QRZ.Models;

namespace QsoManager.Infrastructure.Services.QRZ;

/// <summary>
/// Service de cache pour les sessions QRZ
/// </summary>
public interface IQrzSessionCacheService
{
    /// <summary>
    /// Récupère une session en cache pour un utilisateur donné
    /// </summary>
    /// <param name="username">Nom d'utilisateur QRZ</param>
    /// <returns>Session en cache si valide, null sinon</returns>
    QrzSessionCache? GetCachedSession(string username);
    
    /// <summary>
    /// Stocke une session en cache
    /// </summary>
    /// <param name="username">Nom d'utilisateur QRZ</param>
    /// <param name="sessionKey">Clé de session</param>
    /// <param name="expirationDate">Date d'expiration</param>
    void CacheSession(string username, string sessionKey, DateTime expirationDate);
    
    /// <summary>
    /// Supprime une session du cache
    /// </summary>
    /// <param name="username">Nom d'utilisateur QRZ</param>
    void RemoveSession(string username);
    
    /// <summary>
    /// Nettoie les sessions expirées du cache
    /// </summary>
    void CleanupExpiredSessions();
}

/// <summary>
/// Implémentation du service de cache pour les sessions QRZ
/// </summary>
public class QrzSessionCacheService : IQrzSessionCacheService
{
    private readonly ConcurrentDictionary<string, QrzSessionCache> _sessionCache = new();
    private readonly ILogger<QrzSessionCacheService> _logger;
    private readonly object _cleanupLock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;
    
    public QrzSessionCacheService(ILogger<QrzSessionCacheService> logger)
    {
        _logger = logger;
    }
    
    public QrzSessionCache? GetCachedSession(string username)
    {
        if (string.IsNullOrEmpty(username))
            return null;
            
        // Nettoyage périodique des sessions expirées (toutes les 30 minutes)
        if (DateTime.UtcNow.Subtract(_lastCleanup).TotalMinutes > 30)
        {
            CleanupExpiredSessions();
        }
        
        if (_sessionCache.TryGetValue(username, out var session))
        {
            if (session.IsValid)
            {
                _logger.LogDebug("Session QRZ trouvée en cache pour {Username}, expire le {ExpirationDate}", 
                    username, session.ExpirationDate);
                return session;
            }
            else
            {
                _logger.LogDebug("Session QRZ expirée pour {Username}, suppression du cache", username);
                _sessionCache.TryRemove(username, out _);
            }
        }
        
        return null;
    }
    
    public void CacheSession(string username, string sessionKey, DateTime expirationDate)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(sessionKey))
            return;
            
        var sessionCache = new QrzSessionCache
        {
            SessionKey = sessionKey,
            ExpirationDate = expirationDate,
            Username = username
        };
        
        _sessionCache.AddOrUpdate(username, sessionCache, (key, oldValue) => sessionCache);
        
        _logger.LogInformation("Session QRZ mise en cache pour {Username}, expire le {ExpirationDate}", 
            username, expirationDate);
    }
    
    public void RemoveSession(string username)
    {
        if (string.IsNullOrEmpty(username))
            return;
            
        if (_sessionCache.TryRemove(username, out _))
        {
            _logger.LogDebug("Session QRZ supprimée du cache pour {Username}", username);
        }
    }
    
    public void CleanupExpiredSessions()
    {
        lock (_cleanupLock)
        {
            var now = DateTime.UtcNow;
            var expiredSessions = new List<string>();
            
            foreach (var kvp in _sessionCache)
            {
                if (!kvp.Value.IsValid)
                {
                    expiredSessions.Add(kvp.Key);
                }
            }
            
            foreach (var username in expiredSessions)
            {
                _sessionCache.TryRemove(username, out _);
            }
            
            if (expiredSessions.Count > 0)
            {
                _logger.LogInformation("Nettoyage du cache QRZ: {Count} sessions expirées supprimées", 
                    expiredSessions.Count);
            }
            
            _lastCleanup = now;
        }
    }
}
