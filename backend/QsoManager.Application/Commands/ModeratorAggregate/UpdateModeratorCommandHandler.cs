using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Application.Interfaces.Services;
using QsoManager.Domain.Common;
using QsoManager.Domain.Repositories;
using System.Security.Claims;
using System.Threading.Channels;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.ModeratorAggregate;

public class UpdateModeratorCommandHandler : BaseCommandHandler<UpdateModeratorCommandHandler>, ICommandHandler<UpdateModeratorCommand, ModeratorDto>
{
    private readonly IModeratorAggregateRepository _repository;
    private readonly IEncryptionService _encryptionService;

    public UpdateModeratorCommandHandler(
        IModeratorAggregateRepository repository,
        IEncryptionService encryptionService,
        Channel<IEvent> channel,
        ILogger<UpdateModeratorCommandHandler> logger) : base(channel, logger)
    {
        _repository = repository;
        _encryptionService = encryptionService;
    }

    public async Task<Validation<Error, ModeratorDto>> Handle(UpdateModeratorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extraire l'ID utilisateur du ClaimsPrincipal
            var userIdClaim = request.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("ID utilisateur introuvable ou invalide dans les claims");
                return Error.New("Utilisateur non authentifié ou ID utilisateur invalide.");
            }

            _logger.LogInformation("Début de la mise à jour du modérateur {UserId}", userId);

            var moderatorResult = await _repository.GetByIdAsync(userId);
            
            return await moderatorResult.MatchAsync(
                async moderator =>
                {
                    _logger.LogDebug("Modérateur {UserId} trouvé, mise à jour des informations", userId);

                    // Mettre à jour l'email si fourni
                    if (!string.IsNullOrEmpty(request.Email))
                    {
                        var updateEmailResult = moderator.UpdateEmail(request.Email);
                        if (updateEmailResult.IsFail)
                        {
                            return updateEmailResult.Match(
                                _ => Validation<Error, ModeratorDto>.Success(new ModeratorDto(moderator.Id, moderator.CallSign, moderator.Email)),
                                errors => Validation<Error, ModeratorDto>.Fail(errors)
                            );
                        }
                    }                    // Mettre à jour les credentials QRZ si fournis
                    if (!string.IsNullOrEmpty(request.QrzUsername) || !string.IsNullOrEmpty(request.QrzPassword))
                    {
                        _logger.LogInformation("Mise à jour des credentials QRZ - Username: {QrzUsername}, Password fourni: {HasPassword}", 
                            request.QrzUsername, !string.IsNullOrEmpty(request.QrzPassword));
                        
                        // Utiliser les valeurs existantes si les nouvelles ne sont pas fournies
                        string? finalQrzUsername = !string.IsNullOrEmpty(request.QrzUsername) ? request.QrzUsername : moderator.QrzUsername;
                        string? encryptedPassword = moderator.QrzPasswordEncrypted; // Conserver l'existant par défaut
                        
                        if (!string.IsNullOrEmpty(request.QrzPassword))
                        {
                            encryptedPassword = _encryptionService.Encrypt(request.QrzPassword);
                        }

                        var updateQrzResult = moderator.UpdateQrzCredentials(finalQrzUsername, encryptedPassword);
                        if (updateQrzResult.IsFail)
                        {
                            return updateQrzResult.Match(
                                _ => Validation<Error, ModeratorDto>.Success(new ModeratorDto(moderator.Id, moderator.CallSign, moderator.Email)),
                                errors => Validation<Error, ModeratorDto>.Fail(errors)
                            );
                        }
                        
                        _logger.LogInformation("Credentials QRZ mis à jour avec succès - Username: {QrzUsername}", finalQrzUsername);
                    }

                    // Sauvegarder les changements
                    var saveResult = await _repository.SaveAsync(moderator);
                    return saveResult.Match(
                        _ =>
                        {
                            // Dispatcher les événements
                            var events = moderator.GetUncommittedChanges();
                            events.IfSuccess(eventList => DispatchEventsAsync(eventList, cancellationToken));

                            _logger.LogInformation("Modérateur {UserId} mis à jour avec succès", userId);
                            return Validation<Error, ModeratorDto>.Success(
                                new ModeratorDto(moderator.Id, moderator.CallSign, moderator.Email));
                        },
                        errors =>
                        {
                            _logger.LogError("Erreur lors de la sauvegarde du modérateur {UserId}: {Errors}", 
                                userId, string.Join(", ", errors.Select(e => e.Message)));
                            return Validation<Error, ModeratorDto>.Fail(errors);
                        }
                    );
                },                errors =>
                {
                    _logger.LogError("Erreur lors de la récupération du modérateur {UserId}: {Errors}", 
                        userId, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, ModeratorDto>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de la mise à jour du modérateur");
            return Error.New("Impossible de mettre à jour le modérateur.");
        }
    }
}
