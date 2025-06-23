using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Commands.QsoAggregate;
using QsoManager.Application.DTOs;
using QsoManager.Application.Queries.QsoAggregate;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QsoAggregateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QsoAggregateController> _logger;

    public QsoAggregateController(
        IMediator mediator, 
        ILogger<QsoAggregateController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }    [HttpPost]
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
    }    
      [HttpPost("{aggregateId:guid}/participants")]
    [Authorize]
    public async Task<ActionResult<QsoAggregateDto>> AddParticipant(Guid aggregateId, [FromBody] AddParticipantRequest request)
    {
        try
        {
            var command = new AddParticipantCommand(aggregateId, request.CallSign, User);
            var result = await _mediator.Send(command);

            return result.Match<ActionResult<QsoAggregateDto>>(
                qsoDto => Ok(qsoDto),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout du participant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    [HttpDelete("{aggregateId:guid}/participants/{callSign}")]
    public async Task<ActionResult> RemoveParticipant(Guid aggregateId, string callSign)
    {
        try
        {
            var command = new RemoveParticipantCommand(aggregateId, callSign);
            var result = await _mediator.Send(command);

            return result.Match<ActionResult>(
                _ => NoContent(),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du participant");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }    [HttpPut("{aggregateId:guid}/participants/reorder")]
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
