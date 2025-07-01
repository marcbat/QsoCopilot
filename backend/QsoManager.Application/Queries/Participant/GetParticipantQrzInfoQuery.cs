using System.Security.Claims;
using LanguageExt.Common;
using QsoManager.Application.DTOs.Services;

namespace QsoManager.Application.Queries.Participant;

/// <summary>
/// Requête pour récupérer les informations QRZ d'un participant
/// </summary>
/// <param name="CallSign">L'indicatif du participant</param>
/// <param name="CurrentUser">L'utilisateur actuel pour récupérer ses credentials QRZ</param>
public record GetParticipantQrzInfoQuery(
    string CallSign,
    ClaimsPrincipal? CurrentUser = null
) : IQuery<ParticipantQrzInfoDto>;

/// <summary>
/// DTO contenant les informations QRZ d'un participant
/// </summary>
/// <param name="CallSign">L'indicatif du participant</param>
/// <param name="QrzCallsignInfo">Informations de l'indicatif depuis QRZ.com</param>
/// <param name="QrzDxccInfo">Informations DXCC depuis QRZ.com</param>
public record ParticipantQrzInfoDto(
    string CallSign,
    QrzCallsignInfo? QrzCallsignInfo = null,
    QrzDxccInfo? QrzDxccInfo = null
);
