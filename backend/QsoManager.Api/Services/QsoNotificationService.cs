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
    /// Notifier qu'un participant a été ajouté à un QSO
    /// </summary>
    Task NotifyParticipantAdded(Guid qsoId, ParticipantDto participant);

    /// <summary>
    /// Notifier qu'un participant a été supprimé d'un QSO
    /// </summary>
    Task NotifyParticipantRemoved(Guid qsoId, string callSign);

    /// <summary>
    /// Notifier que l'ordre des participants a changé
    /// </summary>
    Task NotifyParticipantsReordered(Guid qsoId, List<ParticipantDto> participants);

    /// <summary>
    /// Notifier qu'un QSO a été mis à jour
    /// </summary>
    Task NotifyQsoUpdated(Guid qsoId, QsoAggregateDto qso);
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
    }

    public async Task NotifyParticipantAdded(Guid qsoId, ParticipantDto participant)
    {        try
        {
            var groupName = $"qso_{qsoId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ParticipantAdded", new
            {
                QsoId = qsoId,
                Participant = participant
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de la notification ParticipantAdded pour QSO {QsoId}", qsoId);
        }
    }

    public async Task NotifyParticipantRemoved(Guid qsoId, string callSign)
    {        try
        {
            var groupName = $"qso_{qsoId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ParticipantRemoved", new
            {
                QsoId = qsoId,
                CallSign = callSign
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de la notification ParticipantRemoved pour QSO {QsoId}", qsoId);
        }
    }

    public async Task NotifyParticipantsReordered(Guid qsoId, List<ParticipantDto> participants)
    {        try
        {
            var groupName = $"qso_{qsoId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ParticipantsReordered", new
            {
                QsoId = qsoId,
                Participants = participants
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de la notification ParticipantsReordered pour QSO {QsoId}", qsoId);
        }
    }

    public async Task NotifyQsoUpdated(Guid qsoId, QsoAggregateDto qso)
    {        try
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
}
