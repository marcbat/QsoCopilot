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
    }public async Task<Validation<Error, IEnumerable<QsoAggregateDto>>> Handle(
        GetAllQsoAggregatesQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Récupération de tous les QSO Aggregates");

            var result = await _projectionRepository.GetAllAsync(cancellationToken);

            return result.Match(                projections =>
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de tous les QSO Aggregates");
            return Error.New("Impossible de récupérer les QSO Aggregates");
        }
    }
}
