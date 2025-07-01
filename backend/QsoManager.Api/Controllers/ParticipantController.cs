using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Queries.Participant;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParticipantController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ParticipantController> _logger;

    public ParticipantController(
        IMediator mediator, 
        ILogger<ParticipantController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Récupère les informations QRZ d'un participant par son indicatif
    /// </summary>
    /// <param name="callSign">L'indicatif du participant</param>
    /// <returns>Les informations QRZ du participant</returns>
    [HttpGet("{callSign}/qrz")]
    public async Task<ActionResult<ParticipantQrzInfoDto>> GetQrzInfo(string callSign)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(callSign))
            {
                return BadRequest(new { Message = "CallSign parameter is required" });
            }

            _logger.LogInformation("Récupération des informations QRZ pour le participant {CallSign}", callSign);

            var query = new GetParticipantQrzInfoQuery(callSign, User);
            var result = await _mediator.Send(query);

            return result.Match<ActionResult<ParticipantQrzInfoDto>>(
                qrzInfo => Ok(qrzInfo),
                errors => 
                {
                    _logger.LogWarning("Erreur lors de la récupération des informations QRZ pour {CallSign}: {Errors}", 
                        callSign, string.Join(", ", errors.Select(e => e.ToString())));
                    
                    // En cas d'erreur, retourner les informations de base sans enrichissement QRZ
                    var fallbackInfo = new ParticipantQrzInfoDto(callSign);
                    return Ok(fallbackInfo);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des informations QRZ pour {CallSign}", callSign);
            
            // En cas d'erreur, retourner les informations de base sans enrichissement QRZ
            var fallbackInfo = new ParticipantQrzInfoDto(callSign);
            return Ok(fallbackInfo);
        }
    }
}
