using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Application.Projections.Interfaces;

namespace QsoManager.Application.Queries.QsoAggregate;

public class SearchQsoAggregatesByModeratorQueryHandler : IQueryHandler<SearchQsoAggregatesByModeratorQuery, IEnumerable<QsoAggregateDto>>
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<SearchQsoAggregatesByModeratorQueryHandler> _logger;

    public SearchQsoAggregatesByModeratorQueryHandler(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<SearchQsoAggregatesByModeratorQueryHandler> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    public async Task<Validation<Error, IEnumerable<QsoAggregateDto>>> Handle(
        SearchQsoAggregatesByModeratorQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recherche des QSO Aggregates modérés par l'utilisateur '{ModeratorId}'", request.ModeratorId);

            var result = await _projectionRepository.SearchByModeratorAsync(request.ModeratorId, cancellationToken);

            return result.Match(
                projections =>
                {
                    _logger.LogInformation("Repository returned {Count} projections for moderator '{ModeratorId}'", 
                        projections.Count(), request.ModeratorId);
                    
                    var dtos = projections.Select(projection =>
                    {
                        _logger.LogInformation("Mapping projection: {Id} - {Name} (Moderator: {ModeratorId})", 
                            projection.Id, projection.Name, projection.ModeratorId);
                        
                        // Créer les participants de base sans enrichissement QRZ
                        var participants = projection.Participants?.Select(p => new ParticipantDto(p.CallSign, p.Order))
                            .ToList() ?? new List<ParticipantDto>();                        var dto = new QsoAggregateDto(
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
                        
                        _logger.LogInformation("Created DTO: {Id} - {Name} (Moderator: {ModeratorId})", 
                            dto.Id, dto.Name, dto.ModeratorId);
                        return dto;
                    }).ToList();

                    _logger.LogInformation("Handler returning {Count} DTOs for moderator '{ModeratorId}'", 
                        dtos.Count, request.ModeratorId);
                    return Validation<Error, IEnumerable<QsoAggregateDto>>.Success(dtos.AsEnumerable());
                },
                errors => 
                {
                    _logger.LogError("Repository returned errors for moderator '{ModeratorId}': {Errors}", 
                        request.ModeratorId, string.Join(", ", errors.Select(e => e.Message)));
                    return Validation<Error, IEnumerable<QsoAggregateDto>>.Fail(errors);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des QSO Aggregates pour le modérateur '{ModeratorId}'", request.ModeratorId);
            return Error.New($"Impossible de rechercher les QSO Aggregates pour le modérateur '{request.ModeratorId}'");
        }
    }
}
