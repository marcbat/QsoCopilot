using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Commands.QsoAggregate;
using QsoManager.Application.DTOs;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QsoAggregateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QsoAggregateController> _logger;

    public QsoAggregateController(IMediator mediator, ILogger<QsoAggregateController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<QsoAggregateDto>> Create([FromBody] CreateQsoAggregateRequest request)
    {
        try
        {            var command = new CreateQsoAggregateCommand(
                request.Id ?? Guid.NewGuid(),
                request.Name,
                request.Description,
                request.ModeratorId
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
    public async Task<ActionResult> AddParticipant(Guid aggregateId, [FromBody] AddParticipantRequest request)
    {
        try
        {
            var command = new AddParticipantCommand(aggregateId, request.CallSign);
            var result = await _mediator.Send(command);

            return result.Match<ActionResult>(
                _ => NoContent(),
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
    }

    [HttpPut("{aggregateId:guid}/participants/reorder")]
    public async Task<ActionResult> ReorderParticipants(Guid aggregateId, [FromBody] ReorderParticipantsRequest request)
    {
        try
        {
            var command = new ReorderParticipantsCommand(aggregateId, request.NewOrders);
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
public record CreateQsoAggregateRequest(Guid? Id, string Name, string Description, Guid ModeratorId);
public record AddParticipantRequest(string CallSign);
public record ReorderParticipantsRequest(Dictionary<string, int> NewOrders);
public record MoveParticipantRequest(int NewPosition);
