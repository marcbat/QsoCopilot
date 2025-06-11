using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;

namespace QsoManager.Application.Commands.QsoAggregate;

public record RemoveParticipantCommand(
    Guid AggregateId,
    string CallSign
) : ICommand;
