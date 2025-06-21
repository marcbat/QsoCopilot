using System.Security.Claims;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs.Services;
using QsoManager.Application.Interfaces.Services;
using QsoManager.Application.Queries.Participant;
using QsoManager.Domain.Repositories;

namespace QsoManager.Application.Queries.Participant;

/// <summary>
/// Handler pour récupérer les informations QRZ d'un participant
/// </summary>
public class GetParticipantQrzInfoQueryHandler : IQueryHandler<GetParticipantQrzInfoQuery, ParticipantQrzInfoDto>
{
    private readonly IQrzService _qrzService;
    private readonly IModeratorAggregateRepository _moderatorRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<GetParticipantQrzInfoQueryHandler> _logger;

    public GetParticipantQrzInfoQueryHandler(
        IQrzService qrzService,
        IModeratorAggregateRepository moderatorRepository,
        IEncryptionService encryptionService,
        ILogger<GetParticipantQrzInfoQueryHandler> logger)
    {
        _qrzService = qrzService;
        _moderatorRepository = moderatorRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<Validation<Error, ParticipantQrzInfoDto>> Handle(
        GetParticipantQrzInfoQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération des informations QRZ pour le participant {CallSign}", request.CallSign);

            // Récupérer les credentials QRZ de l'utilisateur connecté
            var (qrzUsername, qrzPassword) = await GetUserQrzCredentialsAsync(request.CurrentUser);

            // Lookup callsign information
            var qrzCallsignInfo = await _qrzService.LookupCallsignAsync(
                request.CallSign, 
                qrzUsername, 
                qrzPassword);

            QrzDxccInfo? qrzDxccInfo = null;

            // Si on a un DXCC ID et des credentials, faire un second lookup pour les infos DXCC
            if (qrzCallsignInfo?.Dxcc.HasValue == true && 
                !string.IsNullOrEmpty(qrzUsername) && 
                !string.IsNullOrEmpty(qrzPassword))
            {
                qrzDxccInfo = await _qrzService.LookupDxccAsync(
                    qrzCallsignInfo.Dxcc.Value, 
                    qrzUsername, 
                    qrzPassword);
            }

            var result = new ParticipantQrzInfoDto(
                request.CallSign,
                qrzCallsignInfo,
                qrzDxccInfo
            );

            _logger.LogInformation("Informations QRZ récupérées avec succès pour {CallSign}", request.CallSign);

            return Validation<Error, ParticipantQrzInfoDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des informations QRZ pour {CallSign}", request.CallSign);
            
            // En cas d'erreur, retourner les informations de base sans enrichissement QRZ
            var fallbackResult = new ParticipantQrzInfoDto(request.CallSign);
            return Validation<Error, ParticipantQrzInfoDto>.Success(fallbackResult);
        }
    }

    private async Task<(string? username, string? password)> GetUserQrzCredentialsAsync(ClaimsPrincipal? currentUser)
    {
        if (currentUser == null)
        {
            _logger.LogDebug("Aucun utilisateur connecté, pas de credentials QRZ disponibles");
            return (null, null);
        }

        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogDebug("ID utilisateur invalide ou manquant: {UserIdClaim}", userIdClaim);
            return (null, null);
        }

        var moderatorResult = await _moderatorRepository.GetByIdAsync(userId);
          return moderatorResult.Match(
            moderator =>
            {
                var qrzUsername = moderator.QrzUsername;
                string? qrzPassword = null;
                
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
                else
                {
                    _logger.LogDebug("Aucun mot de passe QRZ chiffré trouvé pour l'utilisateur {UserId}", userId);
                }

                return (qrzUsername, qrzPassword);
            },
            errors =>
            {
                _logger.LogWarning("Impossible de récupérer le modérateur pour l'utilisateur {UserId}: {Errors}", 
                    userId, string.Join(", ", errors.Select(e => e.ToString())));
                return ((string?)null, (string?)null);
            });
    }
}
