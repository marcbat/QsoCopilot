using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Commands.ModeratorAggregate;
using QsoManager.Application.DTOs;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModeratorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ModeratorController> _logger;

    public ModeratorController(IMediator mediator, ILogger<ModeratorController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ModeratorDto>> Create([FromBody] CreateModeratorCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return result.Match<ActionResult<ModeratorDto>>(
                success => Ok(success),
                errors => BadRequest(new { message = string.Join(", ", errors.Select(e => e.Message)) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating moderator {CallSign}", command.CallSign);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
