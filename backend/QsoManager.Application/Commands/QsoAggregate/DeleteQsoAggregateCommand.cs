using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using System.Security.Claims;

namespace QsoManager.Application.Commands.QsoAggregate;

public record DeleteQsoAggregateCommand(
    Guid AggregateId,
    ClaimsPrincipal User
) : ICommand<Unit>;
