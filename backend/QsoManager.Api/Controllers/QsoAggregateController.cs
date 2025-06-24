using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Commands.QsoAggregate;
using QsoManager.Application.Common;
using QsoManager.Application.DTOs;
using QsoManager.Application.Queries.QsoAggregate;
using QsoManager.Api.Services;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QsoAggregateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QsoAggregateController> _logger;
    private readonly IQsoNotificationService _notificationService;

    public QsoAggregateController(
        IMediator mediator, 
        ILogger<QsoAggregateController> logger,
        IQsoNotificationService notificationService)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationService = notificationService;
    }[HttpPost]
    [Authorize]
    public async Task<ActionResult<QsoAggregateDto>> Create([FromBody] CreateQsoAggregateRequest request)
    {
        try
        {            var command = new CreateQsoAggregateCommand(
                request.Id ?? Guid.NewGuid(),
                request.Name,
                request.Description,
                request.Frequency,
                User // Passer le ClaimsPrincipal au lieu du ModeratorId
            );

            var result = await _mediator.Send(command);

            return result.Match<ActionResult<QsoAggregateDto>>(
                dto => Ok(dto),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du QSO Aggregate");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }        [HttpPost("{aggregateId:guid}/participants")]
    [Authorize]
    public async Task<ActionResult<QsoAggregateDto>> AddParticipant(Guid aggregateId, [FromBody] AddParticipantRequest request)
    {
        try
        {
            var command = new AddParticipantCommand(aggregateId, request.CallSign, User);
            var result = await _mediator.Send(command);            return await result.Match<Task<ActionResult<QsoAggregateDto>>>(
                async qsoDto =>
                {                    // Envoyer une notification SignalR
                    var newParticipant = qsoDto.Participants.FirstOrDefault(p => p.CallSign.Equals(request.CallSign, StringComparison.OrdinalIgnoreCase));
                    if (newParticipant != null)
                    {
                        await _notificationService.NotifyParticipantAdded(aggregateId, newParticipant);
                    }
                    
                    return Ok(qsoDto);
                },
                async errors => 
                {
                    await Task.CompletedTask;
                    return BadRequest(new { Errors = errors.Select(e => e.Message) });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout du participant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }    [HttpDelete("{aggregateId:guid}/participants/{callSign}")]
    public async Task<ActionResult> RemoveParticipant(Guid aggregateId, string callSign)
    {
        try
        {
            var command = new RemoveParticipantCommand(aggregateId, callSign);
            var result = await _mediator.Send(command);

            return await result.Match<Task<ActionResult>>(
                async _ => 
                {
                    // Envoyer une notification SignalR
                    await _notificationService.NotifyParticipantRemoved(aggregateId, callSign);
                    return NoContent();
                },
                async errors => 
                {
                    await Task.CompletedTask;
                    return BadRequest(new { Errors = errors.Select(e => e.Message) });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du participant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }[HttpPut("{aggregateId:guid}/participants/reorder")]
    [Authorize]
    public async Task<ActionResult> ReorderParticipants(Guid aggregateId, [FromBody] ReorderParticipantsRequest request)
    {
        try
        {
            var command = new ReorderParticipantsCommand(aggregateId, request.NewOrders, User);
            var result = await _mediator.Send(command);

            return result.Match<ActionResult>(
                _ => NoContent(),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la réorganisation des participants");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    [HttpPut("{aggregateId:guid}/participants/{callSign}/move")]
    public async Task<ActionResult> MoveParticipant(Guid aggregateId, string callSign, [FromBody] MoveParticipantRequest request)
    {
        try
        {
            var command = new MoveParticipantToPositionCommand(aggregateId, callSign, request.NewPosition);
            var result = await _mediator.Send(command);

            return result.Match<ActionResult>(
                _ => NoContent(),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du déplacement du participant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });        }
    }    
      /// <summary>
    /// Récupère tous les QSO Aggregates
    /// </summary>    [HttpGet]
    public async Task<ActionResult<IEnumerable<QsoAggregateDto>>> GetAll()
    {
        try
        {
            var query = new GetAllQsoAggregatesQuery(User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<IEnumerable<QsoAggregateDto>>>(
                qsos => Ok(qsos),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de tous les QSO Aggregates");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère tous les QSO Aggregates avec pagination
    /// </summary>
    [HttpGet("paginated")]
    public async Task<ActionResult<PagedResult<QsoAggregateDto>>> GetAllPaginated(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var pagination = new PaginationParameters 
            { 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };

            if (!pagination.IsValid)
            {
                return BadRequest(new { Message = "Invalid pagination parameters" });
            }

            var query = new GetAllQsoAggregatesWithPaginationQuery(pagination, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<PagedResult<QsoAggregateDto>>>(
                pagedQsos => Ok(pagedQsos),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération paginée de tous les QSO Aggregates");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }    /// <summary>
    /// Récupère un QSO Aggregate par ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<QsoAggregateDto>> GetById(string id)
    {
        try
        {
            // Valider que l'ID est un GUID valide
            if (!Guid.TryParse(id, out var guidId))
            {
                return BadRequest(new { Message = $"Invalid GUID format: {id}" });
            }            var query = new GetQsoAggregateByIdQuery(guidId, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<QsoAggregateDto>>(
                qso => Ok(qso),
                errors => NotFound(new { Message = $"QSO Aggregate with ID {guidId} not found" })
            );        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du QSO Aggregate {Id}", id);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }    /// <summary>
    /// Recherche les QSO par nom
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<QsoAggregateDto>>> SearchByName([FromQuery] string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "Name parameter is required" });
            }

            _logger.LogInformation("Recherche de QSO avec le terme '{SearchTerm}'", name);
            
            var query = new SearchQsoAggregatesByNameQuery(name, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<IEnumerable<QsoAggregateDto>>>(
                success => 
                {
                    _logger.LogInformation("Controller returning {Count} QSO results for term '{SearchTerm}'", success.Count(), name);
                    return Ok(success);
                },
                errors => 
                {
                    _logger.LogError("Search failed with errors: {Errors}", string.Join(", ", errors.Select(e => e.Message)));
                    return StatusCode(500, new { Message = "Erreur lors de la recherche" });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des QSO par nom {Name}", name);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Recherche les QSO par nom avec pagination
    /// </summary>
    [HttpGet("search/paginated")]
    public async Task<ActionResult<PagedResult<QsoAggregateDto>>> SearchByNamePaginated(
        [FromQuery] string name,
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "Name parameter is required" });
            }

            var pagination = new PaginationParameters 
            { 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };

            if (!pagination.IsValid)
            {
                return BadRequest(new { Message = "Invalid pagination parameters" });
            }

            _logger.LogInformation("Recherche paginée de QSO avec le terme '{SearchTerm}' (page {PageNumber}, taille {PageSize})", 
                name, pageNumber, pageSize);
            
            var query = new SearchQsoAggregatesByNameWithPaginationQuery(name, pagination, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<PagedResult<QsoAggregateDto>>>(
                success => 
                {
                    _logger.LogInformation("Controller returning page {PageNumber} of {TotalPages} with {ItemCount} QSO results for term '{SearchTerm}' (total: {TotalCount})", 
                        success.PageNumber, success.TotalPages, success.Items.Count(), name, success.TotalCount);
                    return Ok(success);
                },
                errors => 
                {
                    _logger.LogError("Paginated search failed with errors: {Errors}", string.Join(", ", errors.Select(e => e.Message)));
                    return StatusCode(500, new { Message = "Erreur lors de la recherche paginée" });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche paginée des QSO par nom {Name}", name);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }    /// <summary>
    /// Recherche les QSO modérés par l'utilisateur courant
    /// </summary>
    [HttpGet("my-moderated")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QsoAggregateDto>>> SearchMyModerated()
    {        try
        {
            // Récupérer l'ID de l'utilisateur courant depuis les claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { Message = "Unable to identify current user" });
            }

            _logger.LogInformation("Recherche des QSO modérés par l'utilisateur '{UserId}'", userId);
            
            var query = new SearchQsoAggregatesByModeratorQuery(userId, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<IEnumerable<QsoAggregateDto>>>(
                success => 
                {
                    _logger.LogInformation("Controller returning {Count} QSO results for moderator '{UserId}'", success.Count(), userId);
                    return Ok(success);
                },
                errors => 
                {
                    _logger.LogError("Search by moderator failed with errors: {Errors}", string.Join(", ", errors.Select(e => e.Message)));
                    return StatusCode(500, new { Message = "Erreur lors de la recherche des QSO modérés" });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des QSO modérés par l'utilisateur courant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Recherche les QSO modérés par l'utilisateur courant avec pagination
    /// </summary>
    [HttpGet("my-moderated/paginated")]
    [Authorize]
    public async Task<ActionResult<PagedResult<QsoAggregateDto>>> SearchMyModeratedPaginated(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Récupérer l'ID de l'utilisateur courant depuis les claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { Message = "Unable to identify current user" });
            }

            var pagination = new PaginationParameters 
            { 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };

            if (!pagination.IsValid)
            {
                return BadRequest(new { Message = "Invalid pagination parameters" });
            }

            _logger.LogInformation("Recherche paginée des QSO modérés par l'utilisateur '{UserId}' (page {PageNumber}, taille {PageSize})", 
                userId, pageNumber, pageSize);
            
            var query = new SearchQsoAggregatesByModeratorWithPaginationQuery(userId, pagination, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<PagedResult<QsoAggregateDto>>>(
                success => 
                {
                    _logger.LogInformation("Controller returning page {PageNumber} of {TotalPages} with {ItemCount} QSO results for moderator '{UserId}' (total: {TotalCount})", 
                        success.PageNumber, success.TotalPages, success.Items.Count(), userId, success.TotalCount);
                    return Ok(success);
                },
                errors => 
                {
                    _logger.LogError("Paginated search by moderator failed with errors: {Errors}", string.Join(", ", errors.Select(e => e.Message)));
                    return StatusCode(500, new { Message = "Erreur lors de la recherche paginée des QSO modérés" });
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche paginée des QSO modérés par l'utilisateur courant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }[HttpDelete("{aggregateId:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteQsoAggregate(Guid aggregateId)
    {
        try
        {
            var command = new DeleteQsoAggregateCommand(aggregateId, User);
            var result = await _mediator.Send(command);

            return result.Match<ActionResult>(
                _ => NoContent(),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du QSO Aggregate {AggregateId}", aggregateId);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }
}

// Health Check Controller
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "QSO Manager API"
        });
    }
}

// DTOs pour les requêtes
public record CreateQsoAggregateRequest(Guid? Id, string Name, string? Description, decimal Frequency);
public record AddParticipantRequest(string CallSign);
public record ReorderParticipantsRequest(Dictionary<string, int> NewOrders);
public record MoveParticipantRequest(int NewPosition);
