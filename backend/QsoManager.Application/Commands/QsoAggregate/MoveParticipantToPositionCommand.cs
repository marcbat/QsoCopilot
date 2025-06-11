using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;

namespace QsoManager.Application.Commands.QsoAggregate;

public record MoveParticipantToPositionCommand(
    Guid AggregateId,
    string CallSign,
    int NewPosition
) : ICommand;
