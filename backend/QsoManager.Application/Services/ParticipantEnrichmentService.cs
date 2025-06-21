using QsoManager.Application.DTOs;
using QsoManager.Application.DTOs.Services;
using QsoManager.Application.Interfaces.Services;
using QsoManager.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace QsoManager.Application.Services;

public interface IParticipantEnrichmentService
{
    /// <summary>
    /// Enrichit les données des participants avec les informations QRZ
    /// </summary>
    /// <param name="participants">Les participants à enrichir</param>
    /// <param name="currentUser">L'utilisateur connecté (pour récupérer ses credentials QRZ)</param>
    /// <returns>Les participants enrichis avec les données QRZ</returns>
    Task<IEnumerable<ParticipantDto>> EnrichParticipantsWithQrzDataAsync(
        IEnumerable<ParticipantDto> participants, 
        ClaimsPrincipal? currentUser = null);
}

public class ParticipantEnrichmentService : IParticipantEnrichmentService
{
    private readonly IQrzService _qrzService;
    private readonly IModeratorAggregateRepository _moderatorRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ParticipantEnrichmentService> _logger;

    public ParticipantEnrichmentService(
        IQrzService qrzService,
        IModeratorAggregateRepository moderatorRepository,
        IEncryptionService encryptionService,
        ILogger<ParticipantEnrichmentService> logger)
    {
        _qrzService = qrzService;
        _moderatorRepository = moderatorRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }public async Task<IEnumerable<ParticipantDto>> EnrichParticipantsWithQrzDataAsync(
        IEnumerable<ParticipantDto> participants, 
        ClaimsPrincipal? currentUser = null)
    {
        try
        {
            string? qrzUsername = null;
            string? qrzPassword = null;

            // Si un utilisateur est connecté, récupérer ses credentials QRZ
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    var moderatorResult = await _moderatorRepository.GetByIdAsync(userId);                    moderatorResult.IfSuccess(moderator =>
                    {
                        qrzUsername = moderator.QrzUsername;
                        
                        // Déchiffrer le mot de passe QRZ pour pouvoir l'utiliser avec l'API
                        if (!string.IsNullOrEmpty(moderator.QrzPasswordEncrypted))
                        {
                            try
                            {
                                qrzPassword = _encryptionService.Decrypt(moderator.QrzPasswordEncrypted);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Impossible de déchiffrer le mot de passe QRZ pour l'utilisateur {UserId}", userId);
                                qrzPassword = null;
                            }
                        }
                    });
                }
            }

            var enrichedParticipants = new List<ParticipantDto>();

            foreach (var participant in participants)
            {
                try
                {
                    _logger.LogDebug("Enrichissement QRZ pour le participant {CallSign}", participant.CallSign);

                    // Lookup callsign
                    var qrzCallsignInfo = await _qrzService.LookupCallsignAsync(
                        participant.CallSign, 
                        qrzUsername, 
                        qrzPassword);

                    QrzDxccInfo? qrzDxccInfo = null;

                    // Si on a un DXCC ID, faire un second lookup pour les infos DXCC
                    if (qrzCallsignInfo?.Dxcc.HasValue == true)
                    {
                        qrzDxccInfo = await _qrzService.LookupDxccAsync(
                            qrzCallsignInfo.Dxcc.Value, 
                            qrzUsername, 
                            qrzPassword);
                    }

                    // Créer le participant enrichi
                    var enrichedParticipant = participant with
                    {
                        QrzInfo = qrzCallsignInfo,
                        QrzDxccInfo = qrzDxccInfo
                    };

                    enrichedParticipants.Add(enrichedParticipant);

                    _logger.LogDebug("Participant {CallSign} enrichi avec succès. QRZ trouvé: {QrzFound}, DXCC trouvé: {DxccFound}", 
                        participant.CallSign, 
                        qrzCallsignInfo != null, 
                        qrzDxccInfo != null);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erreur lors de l'enrichissement QRZ pour le participant {CallSign}", participant.CallSign);
                    
                    // En cas d'erreur, retourner le participant sans enrichissement
                    enrichedParticipants.Add(participant);
                }
            }

            return enrichedParticipants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur générale lors de l'enrichissement QRZ des participants");
            
            // En cas d'erreur générale, retourner les participants sans enrichissement
            return participants;
        }
    }
}
