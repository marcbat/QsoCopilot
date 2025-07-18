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

            var result = await _projectionRepository.GetByIdAsync(request.Id, cancellationToken);            return result.Match(
                projection =>
                {
                    // Créer les participants de base sans enrichissement QRZ
                    var participants = projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                        .ToList() ?? new List<ParticipantDto>();                    return Validation<Error, QsoAggregateDto>.Success(new QsoAggregateDto(
                        projection.Id,
                        projection.Name,
                        projection.Description,
                        projection.ModeratorId,
                        projection.Frequency,
                        participants.AsReadOnly(),
                        projection.StartDateTime,
                        projection.CreatedAt,
                        projection.History?.AsReadOnly()
                    ));
                },
                errors => Validation<Error, QsoAggregateDto>.Fail(errors)
            );
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du QSO Aggregate avec l'ID {Id}", request.Id);
            return Error.New($"Impossible de récupérer le QSO Aggregate avec l'ID {request.Id}");
        }
    }
}
