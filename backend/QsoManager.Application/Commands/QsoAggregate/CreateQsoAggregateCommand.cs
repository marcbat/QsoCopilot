using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Commands.QsoAggregate;

public record CreateQsoAggregateCommand(
    Guid Id,
    string Name,
    string Description,
    ClaimsPrincipal User
) : ICommand<QsoAggregateDto>;
