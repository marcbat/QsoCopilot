using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using System.Security.Claims;

namespace QsoManager.Application.Commands.QsoAggregate;

public record ReorderParticipantsCommand(
    Guid AggregateId,
    Dictionary<string, int> NewOrders,
    ClaimsPrincipal User
) : ICommand;
