using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;

namespace QsoManager.Application.Commands.ModeratorAggregate;

public record CreateModeratorCommand(
    Guid Id,
    string CallSign,
    string? Email = null
) : ICommand<ModeratorDto>;
