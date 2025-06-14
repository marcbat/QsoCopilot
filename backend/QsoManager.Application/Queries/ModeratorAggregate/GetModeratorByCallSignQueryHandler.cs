using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.DTOs;
using QsoManager.Domain.Repositories;

namespace QsoManager.Application.Queries.ModeratorAggregate;

public class GetModeratorByCallSignQueryHandler : IQueryHandler<GetModeratorByCallSignQuery, ModeratorDto?>
{
    private readonly IModeratorAggregateRepository _repository;
    private readonly ILogger<GetModeratorByCallSignQueryHandler> _logger;

    public GetModeratorByCallSignQueryHandler(
        IModeratorAggregateRepository repository,
        ILogger<GetModeratorByCallSignQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Validation<Error, ModeratorDto?>> Handle(GetModeratorByCallSignQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recherche du modérateur avec CallSign '{CallSign}'", request.CallSign);

            var result = await _repository.GetByCallSignAsync(request.CallSign);
            
            return result.Match(
                moderator => moderator != null 
                    ? new ModeratorDto(moderator.Id, moderator.CallSign, moderator.Email)
                    : (ModeratorDto?)null,
                errors => 
                {
                    _logger.LogError("Erreur lors de la recherche du modérateur avec CallSign '{CallSign}': {Errors}", 
                        request.CallSign, string.Join(", ", errors.Select(e => e.Message)));
                    return Validation<Error, ModeratorDto?>.Fail(errors);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors de la recherche du modérateur avec CallSign '{CallSign}'", request.CallSign);
            return Error.New($"Erreur lors de la recherche du modérateur avec CallSign '{request.CallSign}'");
        }
    }
}
