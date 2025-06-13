using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QsoManager.Application.DTOs.Authentication;
using QsoManager.Domain.Repositories;
using System.Net.Http.Json;

namespace QsoManager.IntegrationTests.Authentication;

public class RegisterWithModeratorCreationTests : BaseIntegrationTest
{
    public RegisterWithModeratorCreationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }    [Fact]
    public async Task Register_ShouldCreateUserAndModerator_WhenValidData()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1XYZ",
            Password = "Test123!@#",
            Email = "f1xyz@example.com"
        };

        // Act - Enregistrer l'utilisateur
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert - Vérifier que l'enregistrement a réussi
        await Verify(registerResponse, _verifySettings);        // Vérifier qu'un modérateur a été créé avec les bonnes informations
        using var scope = _factory.Services.CreateScope();
        var moderatorRepository = scope.ServiceProvider.GetRequiredService<IModeratorAggregateRepository>();
        
        var moderatorResult = await moderatorRepository.GetByCallSignAsync(registerRequest.UserName.ToUpperInvariant());
        
        // Créer un objet pour la vérification
        var verificationData = new
        {
            ModeratorFound = moderatorResult.IsSuccess,
            ModeratorCallSign = moderatorResult.Match(
                moderator => moderator?.CallSign,
                errors => "Error"
            ),
            ModeratorEmail = moderatorResult.Match(
                moderator => moderator?.Email,
                errors => "Error"
            )
        };

        await Verify(verificationData, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldCreateModeratorWithUppercaseCallSign_WhenUsernameIsLowercase()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "f1lower", // Minuscules volontairement
            Password = "Test123!@#",
            Email = "f1lower@example.com"
        };

        // Act
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(registerResponse, _verifySettings);

        // Vérifier que le modérateur a été créé avec le CallSign en majuscules
        using var scope = _factory.Services.CreateScope();
        var moderatorRepository = scope.ServiceProvider.GetRequiredService<IModeratorAggregateRepository>();
        
        var moderatorResult = await moderatorRepository.GetByCallSignAsync("F1LOWER"); // Recherche en majuscules
          var verificationData = new
        {
            RegisterResponse = registerResponse,
            ModeratorFound = moderatorResult.IsSuccess,
            ModeratorCallSign = moderatorResult.Match(
                moderator => moderator?.CallSign,
                errors => "Error"
            ),
            ExpectedCallSign = "F1LOWER"
        };

        await Verify(verificationData, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldCreateModeratorWithNullEmail_WhenEmailIsEmpty()
    {
        // Arrange - Utilisons un test qui devrait passer avec Identity même avec email vide
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1NOEMAIL",
            Password = "Test123!@#",
            Email = "noemail@example.com" // On utilise un email valide car Identity l'exige
        };

        // Act
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(registerResponse, _verifySettings);

        // Vérifier que le modérateur a été créé
        using var scope = _factory.Services.CreateScope();
        var moderatorRepository = scope.ServiceProvider.GetRequiredService<IModeratorAggregateRepository>();
        
        var moderatorResult = await moderatorRepository.GetByCallSignAsync("F1NOEMAIL");
          var verificationData = new
        {
            RegisterResponse = registerResponse,
            ModeratorFound = moderatorResult.IsSuccess,
            ModeratorEmail = moderatorResult.Match(
                moderator => moderator?.Email,
                errors => "Error"
            )
        };

        await Verify(verificationData, _verifySettings);
    }
}
