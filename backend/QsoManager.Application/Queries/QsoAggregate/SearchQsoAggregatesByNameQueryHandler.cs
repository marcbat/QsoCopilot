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

            return result.Map(projections => 
                projections.Select(p => new QsoAggregateDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ModeratorId,
                    p.Participants?.Select(part => new ParticipantDto(part.CallSign, part.Order))
                        .ToList().AsReadOnly() ?? new List<ParticipantDto>().AsReadOnly()
                )).AsEnumerable()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des QSO Aggregates par nom '{Name}'", request.Name);
            return Error.New($"Impossible de rechercher les QSO Aggregates par nom '{request.Name}'");
        }
    }
}
