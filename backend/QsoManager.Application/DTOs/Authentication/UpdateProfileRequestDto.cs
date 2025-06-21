namespace QsoManager.Application.DTOs.Authentication;

/// <summary>
/// DTO pour la requête de mise à jour du profil utilisateur
/// </summary>
public record UpdateProfileRequestDto(
    string? Email = null,
    string? QrzUsername = null,
    string? QrzPassword = null
);
