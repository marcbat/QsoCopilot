using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Commands.ModeratorAggregate;

/// <summary>
/// Commande pour mettre à jour les informations d'un modérateur
/// </summary>
public record UpdateModeratorCommand(
    string? CallSign = null,
    string? Email = null,
    string? QrzUsername = null,
    string? QrzPassword = null,
    ClaimsPrincipal? User = null
) : ICommand<ModeratorDto>;
