using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;

namespace QsoManager.Application.Queries.QsoAggregate;

public class GetQsoAggregateByIdQueryHandler : IQueryHandler<GetQsoAggregateByIdQuery, QsoAggregateDto>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<GetQsoAggregateByIdQueryHandler> _logger;

    public GetQsoAggregateByIdQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<GetQsoAggregateByIdQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    public async Task<Validation<Error, QsoAggregateDto>> Handle(
        GetQsoAggregateByIdQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération du QSO Aggregate avec l'ID {Id}", request.Id);

            var result = await _projectionRepository.GetByIdAsync(request.Id, cancellationToken);

            return result.Map(projection => new QsoAggregateDto(
                projection.Id,
                projection.Name,
                projection.Description,
                projection.ModeratorId,
                projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                    .ToList().AsReadOnly() ?? new List<ParticipantDto>().AsReadOnly()
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du QSO Aggregate avec l'ID {Id}", request.Id);
            return Error.New($"Impossible de récupérer le QSO Aggregate avec l'ID {request.Id}");
        }
    }
}
