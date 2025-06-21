using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Application.Services;

namespace QsoManager.Application.Queries.QsoAggregate;

public class GetQsoAggregateByIdQueryHandler : IQueryHandler<GetQsoAggregateByIdQuery, QsoAggregateDto>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly IParticipantEnrichmentService _enrichmentService;
    private readonly ILogger<GetQsoAggregateByIdQueryHandler> _logger;

    public GetQsoAggregateByIdQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        IParticipantEnrichmentService enrichmentService,
        ILogger<GetQsoAggregateByIdQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _enrichmentService = enrichmentService;
        _logger = logger;
    }    public async Task<Validation<Error, QsoAggregateDto>> Handle(
        GetQsoAggregateByIdQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération du QSO Aggregate avec l'ID {Id}", request.Id);

            var result = await _projectionRepository.GetByIdAsync(request.Id, cancellationToken);

            return await result.Match(
                async projection =>
                {
                    // Créer les participants de base
                    var baseParticipants = projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                        .ToList() ?? new List<ParticipantDto>();                    // Enrichir avec les données QRZ
                    var enrichedParticipants = await _enrichmentService.EnrichParticipantsWithQrzDataAsync(
                        baseParticipants, 
                        request.CurrentUser);

                    return Validation<Error, QsoAggregateDto>.Success(new QsoAggregateDto(
                        projection.Id,
                        projection.Name,
                        projection.Description,
                        projection.ModeratorId,
                        projection.Frequency,
                        enrichedParticipants.ToList().AsReadOnly(),
                        projection.StartDateTime,
                        projection.CreatedAt
                    ));
                },
                errors => Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du QSO Aggregate avec l'ID {Id}", request.Id);
            return Error.New($"Impossible de récupérer le QSO Aggregate avec l'ID {request.Id}");
        }
    }
}
