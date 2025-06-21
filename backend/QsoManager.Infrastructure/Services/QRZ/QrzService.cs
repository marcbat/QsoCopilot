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
    private string? _sessionKey;

    public QrzService(IHttpClientFactory httpClientFactory, ILogger<QrzService> logger, IOptions<QrzConfiguration> qrzConfiguration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _qrzConfiguration = qrzConfiguration.Value;
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
        }
    }    private async Task<string?> GetSessionKeyAsync(string username, string password)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/current/?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var sessionElement = doc.Root?.Element("Session");
            
            if (sessionElement != null)
            {
                var error = sessionElement.Element("Error")?.Value;
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Erreur QRZ lors de l'authentification: {Error}", error);
                    return null;
                }

                var sessionKey = sessionElement.Element("Key")?.Value;
                if (!string.IsNullOrEmpty(sessionKey))
                {
                    _sessionKey = sessionKey;
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
            var url = $"https://xmldata.qrz.com/xml/current/?s={sessionKey}&callsign={Uri.EscapeDataString(callsign)}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var callsignElement = doc.Root?.Element("Callsign");
            
            if (callsignElement != null)
            {
                return ParseCallsignInfo(callsignElement);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup callsign avec session {Callsign}", callsign);
            return null;
        }
    }    private async Task<QrzCallsignInfo?> LookupCallsignPublicAsync(string callsign)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/current/?callsign={Uri.EscapeDataString(callsign)}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var callsignElement = doc.Root?.Element("Callsign");
            
            if (callsignElement != null)
            {
                return ParseCallsignInfo(callsignElement);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup callsign public {Callsign}", callsign);
            return null;
        }
    }    private async Task<QrzDxccInfo?> LookupDxccWithSessionAsync(int dxccId, string sessionKey)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var url = $"https://xmldata.qrz.com/xml/current/?s={sessionKey}&dxcc={dxccId}";
            var response = await httpClient.GetStringAsync(url);
            
            var doc = XDocument.Parse(response);
            var dxccElement = doc.Root?.Element("DXCC");
            
            if (dxccElement != null)
            {
                return ParseDxccInfo(dxccElement);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du lookup DXCC avec session {DxccId}", dxccId);
            return null;
        }
    }    private QrzCallsignInfo ParseCallsignInfo(XElement callsignElement)
    {
        return new QrzCallsignInfo
        {
            CallSign = callsignElement.Element("call")?.Value ?? string.Empty,
            FName = callsignElement.Element("fname")?.Value,
            Name = callsignElement.Element("name")?.Value,
            Addr1 = callsignElement.Element("addr1")?.Value,
            Addr2 = callsignElement.Element("addr2")?.Value,
            State = callsignElement.Element("state")?.Value,
            Zip = callsignElement.Element("zip")?.Value,
            Country = callsignElement.Element("country")?.Value,
            Lat = ParseDoubleOrNull(callsignElement.Element("lat")?.Value),
            Lon = ParseDoubleOrNull(callsignElement.Element("lon")?.Value),
            Grid = callsignElement.Element("grid")?.Value,
            County = callsignElement.Element("county")?.Value,
            Land = callsignElement.Element("land")?.Value,
            Class = callsignElement.Element("class")?.Value,
            QslManager = callsignElement.Element("qslmgr")?.Value,
            Email = callsignElement.Element("email")?.Value,
            Url = callsignElement.Element("url")?.Value,
            Bio = callsignElement.Element("bio")?.Value,
            Image = callsignElement.Element("image")?.Value,
            Eqsl = callsignElement.Element("eqsl")?.Value,            Mqsl = callsignElement.Element("mqsl")?.Value,
            CqZone = ParseIntOrNull(callsignElement.Element("cqzone")?.Value),
            ItuZone = ParseIntOrNull(callsignElement.Element("ituzone")?.Value),
            Lotw = callsignElement.Element("lotw")?.Value,
            Iota = callsignElement.Element("iota")?.Value,
            GeoLoc = callsignElement.Element("geoloc")?.Value,
            Nickname = callsignElement.Element("nickname")?.Value,
            NameFmt = callsignElement.Element("name_fmt")?.Value,
            Dxcc = ParseIntOrNull(callsignElement.Element("dxcc")?.Value),
            TimeZone = callsignElement.Element("TimeZone")?.Value,
            FetchedAt = DateTime.UtcNow
        };
    }    private QrzDxccInfo ParseDxccInfo(XElement dxccElement)
    {
        return new QrzDxccInfo
        {
            Dxcc = ParseIntOrNull(dxccElement.Element("dxcc")?.Value) ?? 0,
            CountryCode2 = dxccElement.Element("cc")?.Value,
            CountryCode3 = dxccElement.Element("ccc")?.Value,
            Name = dxccElement.Element("name")?.Value,
            Continent = dxccElement.Element("continent")?.Value,
            ItuZone = ParseIntOrNull(dxccElement.Element("ituzone")?.Value),
            CqZone = ParseIntOrNull(dxccElement.Element("cqzone")?.Value),
            TimeZone = dxccElement.Element("timezone")?.Value,
            Lat = ParseDoubleOrNull(dxccElement.Element("lat")?.Value),
            Lon = ParseDoubleOrNull(dxccElement.Element("lon")?.Value),
            Notes = dxccElement.Element("notes")?.Value
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
