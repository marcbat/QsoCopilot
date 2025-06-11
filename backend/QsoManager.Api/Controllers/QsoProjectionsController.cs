using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Application.Projections.Models;
using LanguageExt;
using LanguageExt.Common;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QsoProjectionsController : ControllerBase
{
    private readonly IQsoAggregateProjectionRepository _projectionRepository;
    private readonly ILogger<QsoProjectionsController> _logger;

    public QsoProjectionsController(
        IQsoAggregateProjectionRepository projectionRepository,
        ILogger<QsoProjectionsController> logger)
    {
        _projectionRepository = projectionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Récupère toutes les projections QSO
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QsoAggregateProjectionDto>>> GetAll()
    {
        try
        {
            var result = await _projectionRepository.GetAllAsync();

            return result.Match<ActionResult<IEnumerable<QsoAggregateProjectionDto>>>(
                projections => Ok(projections),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all QSO projections");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère une projection QSO par ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QsoAggregateProjectionDto>> GetById(Guid id)
    {
        try
        {
            var result = await _projectionRepository.GetByIdAsync(id);

            return result.Match<ActionResult<QsoAggregateProjectionDto>>(
                projection => Ok(projection),
                errors => NotFound(new { Message = $"QSO projection with ID {id} not found" })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving QSO projection {Id}", id);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Recherche les projections QSO par nom
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<QsoAggregateProjectionDto>>> SearchByName([FromQuery] string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "Name parameter is required" });
            }

            var result = await _projectionRepository.SearchByNameAsync(name);

            return result.Match<ActionResult<IEnumerable<QsoAggregateProjectionDto>>>(
                projections => Ok(projections),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching QSO projections by name {Name}", name);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }
}
