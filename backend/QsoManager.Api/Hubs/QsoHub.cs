using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace QsoManager.Api.Hubs;

/// <summary>
/// Hub SignalR pour les notifications temps réel des QSO
/// </summary>
// Temporairement sans [Authorize] pour tester
public class QsoHub : Hub
{
    /// <summary>
    /// Rejoindre un groupe de QSO spécifique pour recevoir les notifications
    /// </summary>
    /// <param name="qsoId">ID du QSO à suivre</param>
    public async Task JoinQsoGroup(string qsoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"qso_{qsoId}");
    }

    /// <summary>
    /// Quitter un groupe de QSO
    /// </summary>
    /// <param name="qsoId">ID du QSO à quitter</param>
    public async Task LeaveQsoGroup(string qsoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"qso_{qsoId}");
    }

    /// <summary>
    /// Méthode appelée quand un client se connecte
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Méthode appelée quand un client se déconnecte
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
