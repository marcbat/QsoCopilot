using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;

namespace QsoManager.Application.Commands.QsoAggregate;

public record AssignModeratorCommand(
    Guid AggregateId,
    Guid ModeratorId
) : ICommand;
