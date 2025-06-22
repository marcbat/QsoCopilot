using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;

namespace QsoManager.Application.Queries.QsoAggregate;

public class SearchQsoAggregatesByNameQueryHandler : IQueryHandler<SearchQsoAggregatesByNameQuery, IEnumerable<QsoAggregateDto>>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<SearchQsoAggregatesByNameQueryHandler> _logger;

    public SearchQsoAggregatesByNameQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<SearchQsoAggregatesByNameQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    public async Task<Validation<Error, IEnumerable<QsoAggregateDto>>> Handle(
        SearchQsoAggregatesByNameQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recherche des QSO Aggregates avec le nom '{Name}'", request.Name);

            var result = await _projectionRepository.SearchByNameAsync(request.Name, cancellationToken);

            return result.Match(
                projections =>
                {
                    var dtos = projections.Select(projection =>
                    {
                        // Créer les participants de base sans enrichissement QRZ
                        var participants = projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                            .ToList() ?? new List<ParticipantDto>();

                        return new QsoAggregateDto(
                            projection.Id,
                            projection.Name,
                            projection.Description,
                            projection.ModeratorId,
                            projection.Frequency,
                            participants.AsReadOnly(),
                            projection.StartDateTime,
                            projection.CreatedAt
                        );
                    }).ToList();

                    return Validation<Error, IEnumerable<QsoAggregateDto>>.Success(dtos.AsEnumerable());
                },
                errors => Validation<Error, IEnumerable<QsoAggregateDto>>.Fail(errors)
            );
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des QSO Aggregates par nom '{Name}'", request.Name);
            return Error.New($"Impossible de rechercher les QSO Aggregates par nom '{request.Name}'");
        }
    }
}
