using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;

namespace QsoManager.Application.Commands.QsoAggregate;

public record AddParticipantCommand(
    Guid AggregateId,
    string CallSign
) : ICommand;
