using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;

namespace QsoManager.Application.Commands.QsoAggregate;

public record ReorderParticipantsCommand(
    Guid AggregateId,
    Dictionary<string, int> NewOrders
) : ICommand;
