using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;

namespace QsoManager.Application.Queries.QsoAggregate;

public class GetAllQsoAggregatesQueryHandler : IQueryHandler<GetAllQsoAggregatesQuery, IEnumerable<QsoAggregateDto>>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<GetAllQsoAggregatesQueryHandler> _logger;

    public GetAllQsoAggregatesQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<GetAllQsoAggregatesQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    public async Task<Validation<Error, IEnumerable<QsoAggregateDto>>> Handle(
        GetAllQsoAggregatesQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération de tous les QSO Aggregates");

            var result = await _projectionRepository.GetAllAsync(cancellationToken);            return result.Map(projections =>                projections.Select(p => new QsoAggregateDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ModeratorId,
                    p.Frequency,
                    p.Participants?.Select(part => new ParticipantDto(part.CallSign, part.Order))
                        .ToList().AsReadOnly() ?? new List<ParticipantDto>().AsReadOnly(),
                    p.StartDateTime,
                    p.CreatedAt
                )).AsEnumerable()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de tous les QSO Aggregates");
            return Error.New("Impossible de récupérer les QSO Aggregates");
        }
    }
}
