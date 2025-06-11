using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using QsoManager.Domain.Services;
using System.Threading.Channels;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class CreateQsoAggregateCommandHandler : BaseCommandHandler, ICommandHandler<CreateQsoAggregateCommand, QsoAggregateDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly IQsoAggregateService _domainService;
    private readonly ILogger<CreateQsoAggregateCommandHandler> _logger;

    public CreateQsoAggregateCommandHandler(
        IEventRepository eventRepository,
        IQsoAggregateService domainService,
        Channel<IEvent> channel,
        ILogger<CreateQsoAggregateCommandHandler> logger) : base(channel)
    {
        _eventRepository = eventRepository;
        _domainService = domainService;
        _logger = logger;
    }    public async Task<Validation<Error, QsoAggregateDto>> Handle(CreateQsoAggregateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Exécution du handler de la commande CreateQsoAggregateCommand");            var nameValidation = await _domainService.ValidateUniqueNameAsync(request.Name);
            
            var aggregateResult = 
                from _ in nameValidation
                from aggregate in Domain.Aggregates.QsoAggregate.Create(request.Id, request.Name, request.Description, request.ModeratorId)
                select aggregate;

            return await aggregateResult.MatchAsync(
                async aggregate =>
                {
                    var eventsResult = aggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {
                            var saveResult = await _eventRepository.SaveEventsAsync(events, cancellationToken);
                            return saveResult.Match(
                                _ =>
                                {
                                    DispatchEventsAsync(events, cancellationToken);
                                    var participantDtos = aggregate.Participants
                                        .Select(p => new ParticipantDto(p.CallSign, p.Order))
                                        .ToArray();                                return Validation<Error, QsoAggregateDto>.Success(
                                        new QsoAggregateDto(aggregate.Id, aggregate.Name, aggregate.Description, aggregate.ModeratorId, participantDtos));
                                },
                                errors => Validation<Error, QsoAggregateDto>.Fail(errors)
                            );
                        },
                        errors => Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors))
                    );
                },
                errors => Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur s'est produite lors de l'exécution de la commande CreateQsoAggregateCommand");
            return Error.New("Impossible de créer l'agrégat QSO.");
        }
    }
}
