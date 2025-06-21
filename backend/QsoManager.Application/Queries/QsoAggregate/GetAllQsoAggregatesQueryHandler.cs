using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Application.Services;

namespace QsoManager.Application.Queries.QsoAggregate;

public class GetAllQsoAggregatesQueryHandler : IQueryHandler<GetAllQsoAggregatesQuery, IEnumerable<QsoAggregateDto>>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly IParticipantEnrichmentService _enrichmentService;
    private readonly ILogger<GetAllQsoAggregatesQueryHandler> _logger;

    public GetAllQsoAggregatesQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        IParticipantEnrichmentService enrichmentService,
        ILogger<GetAllQsoAggregatesQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _enrichmentService = enrichmentService;
        _logger = logger;
    }    public async Task<Validation<Error, IEnumerable<QsoAggregateDto>>> Handle(
        GetAllQsoAggregatesQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération de tous les QSO Aggregates");

            var result = await _projectionRepository.GetAllAsync(cancellationToken);

            return await result.Match(
                async projections =>
                {
                    var enrichedDtos = new List<QsoAggregateDto>();
                    
                    foreach (var projection in projections)
                    {
                        // Créer les participants de base
                        var baseParticipants = projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                            .ToList() ?? new List<ParticipantDto>();                        // Enrichir avec les données QRZ
                        var enrichedParticipants = await _enrichmentService.EnrichParticipantsWithQrzDataAsync(
                            baseParticipants, 
                            request.CurrentUser);

                        enrichedDtos.Add(new QsoAggregateDto(
                            projection.Id,
                            projection.Name,
                            projection.Description,
                            projection.ModeratorId,
                            projection.Frequency,
                            enrichedParticipants.ToList().AsReadOnly(),
                            projection.StartDateTime,
                            projection.CreatedAt
                        ));
                    }

                    return Validation<Error, IEnumerable<QsoAggregateDto>>.Success(enrichedDtos.AsEnumerable());
                },
                errors => Task.FromResult(Validation<Error, IEnumerable<QsoAggregateDto>>.Fail(errors))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de tous les QSO Aggregates");
            return Error.New("Impossible de récupérer les QSO Aggregates");
        }
    }
}
