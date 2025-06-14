using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.DTOs;

namespace QsoManager.Application.Queries.ModeratorAggregate;

public record GetModeratorByCallSignQuery(string CallSign) : IQuery<ModeratorDto?>;
