using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Domain.Repositories;
using System.Threading.Channels;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class AddParticipantCommandHandler : BaseCommandHandler<AddParticipantCommandHandler>, ICommandHandler<AddParticipantCommand, QsoAggregateDto>
{
    private readonly IQsoAggregateRepository _repository;

    public AddParticipantCommandHandler(IQsoAggregateRepository repository, Channel<IEvent> channel, ILogger<AddParticipantCommandHandler> logger) : base(channel, logger)
    {
        _repository = repository;
    }    public async Task<Validation<Error, QsoAggregateDto>> Handle(AddParticipantCommand request, CancellationToken cancellationToken)    {
        try
        {
            _logger.LogInformation("Début de l'ajout du participant '{CallSign}' à l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
            
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.AddParticipant(request.CallSign)
                select updatedAggregate;

            return await result.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Participant '{CallSign}' ajouté avec succès à l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
                    
                    var saveResult = await _repository.SaveAsync(aggregate);
                    return saveResult.Match(
                        _ => 
                        {
                            _logger.LogInformation("Participant '{CallSign}' ajouté avec succès et agrégat {AggregateId} sauvegardé", request.CallSign, request.AggregateId);
                            
                            // Créer le DTO avec l'agrégat mis à jour
                            var participantDtos = aggregate.Participants
                                .Select(p => new ParticipantDto(p.CallSign, p.Order))
                                .ToArray();

                            var qsoDto = new QsoAggregateDto(
                                aggregate.Id, 
                                aggregate.Name, 
                                aggregate.Description, 
                                aggregate.ModeratorId, 
                                participantDtos);

                            return Validation<Error, QsoAggregateDto>.Success(qsoDto);
                        },
                        errors =>
                        {
                            _logger.LogError("Erreur lors de la sauvegarde de l'agrégat {AggregateId} après ajout du participant '{CallSign}': {Errors}", request.AggregateId, request.CallSign, string.Join(", ", errors.Select(e => e.Message)));
                            return Validation<Error, QsoAggregateDto>.Fail(errors);
                        }
                    );
                },
                errors => 
                {
                    _logger.LogError("Erreur lors de l'ajout du participant '{CallSign}' à l'agrégat {AggregateId}: {Errors}", request.CallSign, request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de l'ajout du participant '{CallSign}' à l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
            return Error.New("Impossible d'ajouter le participant.");
        }
    }
}
