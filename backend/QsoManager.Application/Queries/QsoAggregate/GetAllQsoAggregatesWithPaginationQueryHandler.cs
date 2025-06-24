using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Common;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;

namespace QsoManager.Application.Queries.QsoAggregate;

public class GetAllQsoAggregatesWithPaginationQueryHandler : IQueryHandler<GetAllQsoAggregatesWithPaginationQuery, PagedResult<QsoAggregateDto>>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<GetAllQsoAggregatesWithPaginationQueryHandler> _logger;

    public GetAllQsoAggregatesWithPaginationQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<GetAllQsoAggregatesWithPaginationQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    public async Task<Validation<Error, PagedResult<QsoAggregateDto>>> Handle(
        GetAllQsoAggregatesWithPaginationQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération paginée de tous les QSO Aggregates (page {PageNumber}, taille {PageSize})",
                request.Pagination.PageNumber, request.Pagination.PageSize);

            var result = await _projectionRepository.GetAllPaginatedAsync(request.Pagination, cancellationToken);

            return result.Match(
                pagedProjections =>
                {
                    var dtos = pagedProjections.Items.Select(projection =>
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

                    var pagedResult = new PagedResult<QsoAggregateDto>(
                        dtos, 
                        pagedProjections.TotalCount, 
                        pagedProjections.PageNumber, 
                        pagedProjections.PageSize);

                    _logger.LogInformation("Retournant page {PageNumber} de {TotalPages} avec {ItemCount} QSO Aggregates (total: {TotalCount})",
                        pagedResult.PageNumber, pagedResult.TotalPages, dtos.Count, pagedResult.TotalCount);

                    return Validation<Error, PagedResult<QsoAggregateDto>>.Success(pagedResult);
                },
                errors => Validation<Error, PagedResult<QsoAggregateDto>>.Fail(errors)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération paginée de tous les QSO Aggregates");
            return Error.New("Impossible de récupérer les QSO Aggregates paginés");
        }
    }
}
