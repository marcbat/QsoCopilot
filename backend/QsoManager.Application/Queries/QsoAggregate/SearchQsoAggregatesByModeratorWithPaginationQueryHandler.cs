using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Common;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;

namespace QsoManager.Application.Queries.QsoAggregate;

public class SearchQsoAggregatesByModeratorWithPaginationQueryHandler : IQueryHandler<SearchQsoAggregatesByModeratorWithPaginationQuery, PagedResult<QsoAggregateDto>>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<SearchQsoAggregatesByModeratorWithPaginationQueryHandler> _logger;

    public SearchQsoAggregatesByModeratorWithPaginationQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<SearchQsoAggregatesByModeratorWithPaginationQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    public async Task<Validation<Error, PagedResult<QsoAggregateDto>>> Handle(
        SearchQsoAggregatesByModeratorWithPaginationQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recherche paginée de QSO modérés par '{ModeratorId}' (page {PageNumber}, taille {PageSize})",
                request.ModeratorId, request.Pagination.PageNumber, request.Pagination.PageSize);

            var result = await _projectionRepository.SearchByModeratorPaginatedAsync(request.ModeratorId, request.Pagination, cancellationToken);

            return result.Match(
                pagedProjections =>
                {
                    var dtos = pagedProjections.Items.Select(projection =>
                    {
                        // Créer les participants de base sans enrichissement QRZ
                        var participants = projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                            .ToList() ?? new List<ParticipantDto>();                        return new QsoAggregateDto(
                            projection.Id,
                            projection.Name,
                            projection.Description,
                            projection.ModeratorId,
                            projection.Frequency,
                            participants.AsReadOnly(),
                            projection.StartDateTime,
                            projection.CreatedAt,
                            projection.History?.AsReadOnly()
                        );
                    }).ToList();

                    var pagedResult = new PagedResult<QsoAggregateDto>(
                        dtos, 
                        pagedProjections.TotalCount, 
                        pagedProjections.PageNumber, 
                        pagedProjections.PageSize);

                    _logger.LogInformation("Retournant page {PageNumber} de {TotalPages} avec {ItemCount} QSO modérés par '{ModeratorId}' (total: {TotalCount})",
                        pagedResult.PageNumber, pagedResult.TotalPages, dtos.Count, request.ModeratorId, pagedResult.TotalCount);

                    return Validation<Error, PagedResult<QsoAggregateDto>>.Success(pagedResult);
                },
                errors => Validation<Error, PagedResult<QsoAggregateDto>>.Fail(errors)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche paginée de QSO modérés par '{ModeratorId}'", request.ModeratorId);
            return Error.New($"Impossible de rechercher les QSO modérés par '{request.ModeratorId}' avec pagination");
        }
    }
}
