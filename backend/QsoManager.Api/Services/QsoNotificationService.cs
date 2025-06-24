using Microsoft.AspNetCore.SignalR;
using QsoManager.Api.Hubs;
using QsoManager.Application.DTOs;

namespace QsoManager.Api.Services;

/// <summary>
/// Service pour envoyer des notifications temps réel via SignalR
/// </summary>
public interface IQsoNotificationService
{
    /// <summary>
    /// Notifier qu'un QSO a été mis à jour
    /// </summary>
    Task NotifyQsoUpdated(Guid qsoId, QsoAggregateDto qso);

    /// <summary>
    /// Notifier qu'il y a eu un changement dans les participants d'un QSO
    /// </summary>
    Task NotifyQsoParticipantsChanged(Guid qsoId, string actionType, string? participantCallSign = null, string? message = null);
}

public class QsoNotificationService : IQsoNotificationService
{
    private readonly IHubContext<QsoHub> _hubContext;
    private readonly ILogger<QsoNotificationService> _logger;

    public QsoNotificationService(
        IHubContext<QsoHub> hubContext,
        ILogger<QsoNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }    public async Task NotifyQsoUpdated(Guid qsoId, QsoAggregateDto qso)
    {
        try
        {
            var groupName = $"qso_{qsoId}";
            await _hubContext.Clients.Group(groupName).SendAsync("QsoUpdated", new
            {
                QsoId = qsoId,
                Qso = qso
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de la notification QsoUpdated pour QSO {QsoId}", qsoId);
        }
    }

    public async Task NotifyQsoParticipantsChanged(Guid qsoId, string actionType, string? participantCallSign = null, string? message = null)
    {
        try
        {
            var groupName = $"qso_{qsoId}";
            await _hubContext.Clients.Group(groupName).SendAsync("QsoParticipantsChanged", new
            {
                QsoId = qsoId,
                ActionType = actionType,
                ParticipantCallSign = participantCallSign,
                Message = message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de la notification QsoParticipantsChanged pour QSO {QsoId}", qsoId);
        }
    }
}
