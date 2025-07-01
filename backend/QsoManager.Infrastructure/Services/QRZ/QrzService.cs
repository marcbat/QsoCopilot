using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QsoManager.Application.DTOs.Services;
using QsoManager.Application.Interfaces.Services;
using QsoManager.Infrastructure.Configuration;
using QsoManager.Infrastructure.Services.QRZ.Models;

namespace QsoManager.Infrastructure.Services.QRZ;

public class QrzService : IQrzService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<QrzService> _logger;
    private readonly QrzConfiguration _qrzConfiguration;
    private readonly IQrzSessionCacheService _sessionCacheService;

    public QrzService(
        IHttpClientFactory httpClientFactory, 
        ILogger<QrzService> logger, 
        IOptions<QrzConfiguration> qrzConfiguration,
        IQrzSessionCacheService sessionCacheService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _qrzConfiguration = qrzConfiguration.Value;
        _sessionCacheService = sessionCacheService;
    }

    public async Task<QrzCallsignInfo?> LookupCallsignAsync(string callsign, string? qrzUsername = null, string? qrzPassword = null)
    {
        try
        {
            // Essayer d'abord avec les credentials utilisateur s'ils sont fournis
            if (!string.IsNullOrEmpty(qrzUsername) && !string.IsNullOrEmpty(qrzPassword))
            {
                var sessionKey = await GetSessionKeyAsync(qrzUsername, qrzPassword);
                if (!string.IsNullOrEmpty(sessionKey))
                {
                    var result = await LookupCallsignWithSessionAsync(callsign, sessionKey);
                    if (result != null)
                        return result;
                }
            }

            // Fallback sans credentials (lookup public limité)
            return await LookupCallsignPublicAsync(callsign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup QRZ pour le callsign {Callsign}", callsign);
            return null;
        }
    }

    public async Task<QrzDxccInfo?> LookupDxccAsync(int dxccId, string? qrzUsername = null, string? qrzPassword = null)
    {
        try
        {
            // Pour le lookup DXCC, on a besoin d'une session authentifiée
            if (string.IsNullOrEmpty(qrzUsername) || string.IsNullOrEmpty(qrzPassword))
            {
                _logger.LogWarning("Credentials QRZ requis pour le lookup DXCC {DxccId}", dxccId);
                return null;
            }

            var sessionKey = await GetSessionKeyAsync(qrzUsername, qrzPassword);
            if (string.IsNullOrEmpty(sessionKey))
            {
                _logger.LogWarning("Impossible d'obtenir une session QRZ pour le lookup DXCC {DxccId}", dxccId);
                return null;
            }

            return await LookupDxccWithSessionAsync(dxccId, sessionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup DXCC QRZ pour l'ID {DxccId}", dxccId);
            return null;
        }    }    private async Task<string?> GetSessionKeyAsync(string username, string password)
    {
        // Vérifier d'abord le cache
        var cachedSession = _sessionCacheService.GetCachedSession(username);
        if (cachedSession != null)
        {
            // Si la session expire bientôt (dans les 30 prochaines minutes), la renouveler proactivement
            if (cachedSession.IsExpiringSoon(30))
            {
                _logger.LogDebug("Session QRZ expire bientôt pour {Username}, renouvellement proactif", username);
                // Continuer pour renouveler la session
            }
            else if (cachedSession.IsValid)
            {
                _logger.LogDebug("Utilisation de la session QRZ en cache pour {Username}", username);
                return cachedSession.SessionKey;
            }
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
            var response = await httpClient.GetStringAsync(url);
              
            var doc = XDocument.Parse(response);
            var ns = XNamespace.Get("http://xmldata.qrz.com");
            var sessionElement = doc.Root?.Element(ns + "Session");
              
            if (sessionElement != null)
            {
                var error = sessionElement.Element(ns + "Error")?.Value;
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Erreur QRZ lors de l'authentification: {Error}", error);
                    // Supprimer la session du cache en cas d'erreur
                    _sessionCacheService.RemoveSession(username);
                    return null;
                }

                var sessionKey = sessionElement.Element(ns + "Key")?.Value;
                var subExpString = sessionElement.Element(ns + "SubExp")?.Value;
                
                if (!string.IsNullOrEmpty(sessionKey))
                {
                    // Calculer la date d'expiration
                    DateTime expirationDate = DateTime.UtcNow.AddHours(24); // Par défaut 24h
                    
                    if (!string.IsNullOrEmpty(subExpString) && DateTime.TryParse(subExpString, out var parsedExp))
                    {
                        expirationDate = parsedExp;
                        _logger.LogDebug("Session QRZ expire le {ExpirationDate} selon SubExp", expirationDate);
                    }
                    else
                    {
                        _logger.LogDebug("SubExp non disponible, utilisation d'une expiration par défaut de 24h");
                    }
                    
                    // Mettre en cache la session
                    _sessionCacheService.CacheSession(username, sessionKey, expirationDate);
                    
                    return sessionKey;
                }
            }

            _logger.LogWarning("Aucune clé de session reçue de QRZ.com");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'authentification QRZ");
            return null;
        }
    }    private async Task<QrzCallsignInfo?> LookupCallsignWithSessionAsync(string callsign, string sessionKey)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/?s={sessionKey}&callsign={Uri.EscapeDataString(callsign)}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var ns = XNamespace.Get("http://xmldata.qrz.com");
            var callsignElement = doc.Root?.Element(ns + "Callsign");
            
            if (callsignElement != null)
            {
                return ParseCallsignInfo(callsignElement);
            }

            return null;
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup callsign avec session {Callsign}", callsign);
            return null;
        }
    }    private async Task<QrzCallsignInfo?> LookupCallsignPublicAsync(string callsign)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/?callsign={Uri.EscapeDataString(callsign)}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var ns = XNamespace.Get("http://xmldata.qrz.com");
            var callsignElement = doc.Root?.Element(ns + "Callsign");
            
            if (callsignElement != null)
            {
                return ParseCallsignInfo(callsignElement);
            }

            return null;
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup callsign public {Callsign}", callsign);
            return null;
        }
    }    private async Task<QrzDxccInfo?> LookupDxccWithSessionAsync(int dxccId, string sessionKey)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/?s={sessionKey}&dxcc={dxccId}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var ns = XNamespace.Get("http://xmldata.qrz.com");
            var dxccElement = doc.Root?.Element(ns + "DXCC");
            
            if (dxccElement != null)
            {
                return ParseDxccInfo(dxccElement);
            }

            return null;
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup DXCC avec session {DxccId}", dxccId);
            return null;
        }
    }

    private QrzCallsignInfo ParseCallsignInfo(XElement callsignElement)
    {
        var ns = XNamespace.Get("http://xmldata.qrz.com");
        return new QrzCallsignInfo
        {
            CallSign = callsignElement.Element(ns + "call")?.Value ?? string.Empty,
            FName = callsignElement.Element(ns + "fname")?.Value,
            Name = callsignElement.Element(ns + "name")?.Value,
            Addr1 = callsignElement.Element(ns + "addr1")?.Value,
            Addr2 = callsignElement.Element(ns + "addr2")?.Value,
            State = callsignElement.Element(ns + "state")?.Value,
            Zip = callsignElement.Element(ns + "zip")?.Value,
            Country = callsignElement.Element(ns + "country")?.Value,
            Lat = ParseDoubleOrNull(callsignElement.Element(ns + "lat")?.Value),
            Lon = ParseDoubleOrNull(callsignElement.Element(ns + "lon")?.Value),
            Grid = callsignElement.Element(ns + "grid")?.Value,
            County = callsignElement.Element(ns + "county")?.Value,
            Land = callsignElement.Element(ns + "land")?.Value,
            Class = callsignElement.Element(ns + "class")?.Value,
            QslManager = callsignElement.Element(ns + "qslmgr")?.Value,            Email = callsignElement.Element(ns + "email")?.Value,
            Url = callsignElement.Element(ns + "url")?.Value,
            Bio = callsignElement.Element(ns + "bio")?.Value,
            Image = callsignElement.Element(ns + "image")?.Value,
            Eqsl = callsignElement.Element(ns + "eqsl")?.Value,
            Mqsl = callsignElement.Element(ns + "mqsl")?.Value,
            CqZone = ParseIntOrNull(callsignElement.Element(ns + "cqzone")?.Value),
            ItuZone = ParseIntOrNull(callsignElement.Element(ns + "ituzone")?.Value),
            Lotw = callsignElement.Element(ns + "lotw")?.Value,
            Iota = callsignElement.Element(ns + "iota")?.Value,
            GeoLoc = callsignElement.Element(ns + "geoloc")?.Value,
            Nickname = callsignElement.Element(ns + "nickname")?.Value,
            NameFmt = callsignElement.Element(ns + "name_fmt")?.Value,            Dxcc = ParseIntOrNull(callsignElement.Element(ns + "dxcc")?.Value),
            TimeZone = callsignElement.Element(ns + "TimeZone")?.Value,
            FetchedAt = DateTime.UtcNow
        };
    }

    private QrzDxccInfo ParseDxccInfo(XElement dxccElement)
    {
        var ns = XNamespace.Get("http://xmldata.qrz.com");
        return new QrzDxccInfo
        {
            Dxcc = ParseIntOrNull(dxccElement.Element(ns + "dxcc")?.Value) ?? 0,
            CountryCode2 = dxccElement.Element(ns + "cc")?.Value,
            CountryCode3 = dxccElement.Element(ns + "ccc")?.Value,
            Name = dxccElement.Element(ns + "name")?.Value,
            Continent = dxccElement.Element(ns + "continent")?.Value,
            ItuZone = ParseIntOrNull(dxccElement.Element(ns + "ituzone")?.Value),
            CqZone = ParseIntOrNull(dxccElement.Element(ns + "cqzone")?.Value),
            TimeZone = dxccElement.Element(ns + "timezone")?.Value,
            Lat = ParseDoubleOrNull(dxccElement.Element(ns + "lat")?.Value),
            Lon = ParseDoubleOrNull(dxccElement.Element(ns + "lon")?.Value),
            Notes = dxccElement.Element(ns + "notes")?.Value
        };
    }

    private static int? ParseIntOrNull(string? value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }

    private static double? ParseDoubleOrNull(string? value)
    {
        return double.TryParse(value, out var result) ? result : null;
    }

    private static DateTime? ParseDateTimeOrNull(string? value)
    {
        return DateTime.TryParse(value, out var result) ? result : null;
    }
}
