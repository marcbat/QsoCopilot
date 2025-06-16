using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Commands.QsoAggregate;

public record AddParticipantCommand(
    Guid AggregateId,
    string CallSign,
    ClaimsPrincipal User
) : ICommand<QsoAggregateDto>;
