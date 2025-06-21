using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using QsoManager.IntegrationTests.Helpers;
using System.Net.Http.Json;
using Xunit;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerQrzEnrichmentTests : BaseIntegrationTest
{
    public QsoAggregateControllerQrzEnrichmentTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) 
        : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task GetQso_WithQrzCredentials_ShouldEnrichParticipantsWithQrzData()
    {
        // Arrange
        var testCallsign = "F4QRZ";
        var (userId, token) = await CreateAndAuthenticateUserAsync(testCallsign);        
        
        // 1. Vérifier que les credentials QRZ sont disponibles
        var qrzCredentials = QrzSecretsHelper.GetQrzCredentials();
        if (qrzCredentials == null)
        {
            // Skip le test si les credentials QRZ ne sont pas disponibles
            Console.WriteLine("SKIPPED: QRZ credentials not available. Configure QRZ credentials using:");
            Console.WriteLine("dotnet user-secrets set 'QrzCredentials:Username' 'your_qrz_username'");
            Console.WriteLine("dotnet user-secrets set 'QrzCredentials:Password' 'your_qrz_password'");
            Console.WriteLine("Or set environment variables QRZ_TEST_USERNAME and QRZ_TEST_PASSWORD");
            return;
        }
        
        var (qrzUsername, qrzPassword) = qrzCredentials.Value;

        // 2. Mettre à jour le profil utilisateur avec des credentials QRZ
        var updateProfileRequest = new
        {
            Email = "test@example.com",
            QrzUsername = qrzUsername,
            QrzPassword = qrzPassword
        };

        var profileResponse = await _client.PutAsJsonAsync("/api/auth/profile", updateProfileRequest);
        
        // 3. Créer un QSO
        var qsoId = Guid.NewGuid();
        var createQsoRequest = new
        {
            Id = qsoId,
            Name = "QSO Test QRZ",
            Description = "QSO avec enrichissement QRZ",
            Frequency = 145.800m
        };

        var createQsoResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createQsoRequest);        // 4. Ajouter un participant avec un callsign valide QRZ (différent de l'utilisateur connecté)
        var addParticipantRequest = new
        {
            CallSign = "HB3XAJ"
        };

        var addParticipantResponse = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Attendre que les projections soient mises à jour et l'enrichissement soit fait
        await Task.Delay(500);        // 5. Récupérer le QSO pour vérifier l'enrichissement (gardons l'authentification pour l'enrichissement QRZ)
        var getQsoResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert - Utiliser Verify pour capturer le snapshot avec enrichissement QRZ
        await Verify(getQsoResponse, _verifySettings)
            .UseParameters("WithQrzCredentials");
    }

    [Fact]
    public async Task GetQso_WithoutQrzCredentials_ShouldReturnParticipantsWithNullQrzData()
    {
        // Arrange
        var testCallsign = "F4NOQRZ";
        var (userId, token) = await CreateAndAuthenticateUserAsync(testCallsign);
        
        // Ne pas ajouter de credentials QRZ pour cet utilisateur

        // Créer un QSO
        var qsoId = Guid.NewGuid();
        var createQsoRequest = new
        {
            Id = qsoId,
            Name = "QSO Test No QRZ",
            Description = "QSO sans credentials QRZ",
            Frequency = 145.800m
        };

        var createQsoResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createQsoRequest);

        // Ajouter un participant
        var addParticipantRequest = new
        {
            CallSign = "F4TEST"
        };

        var addParticipantResponse = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Attendre que les projections soient mises à jour
        await Task.Delay(200);

        // Récupérer le QSO
        ClearAuthentication();
        var getQsoResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert - Utiliser Verify pour capturer le snapshot sans enrichissement QRZ
        await Verify(getQsoResponse, _verifySettings)
            .UseParameters("WithoutQrzCredentials");
    }

    [Fact]
    public async Task UpdateProfile_WithQrzCredentials_ShouldSucceed()
    {
        // Arrange
        var testCallsign = "F4PROFILE";
        var (userId, token) = await CreateAndAuthenticateUserAsync(testCallsign);

        // Act - Mettre à jour le profil avec des credentials QRZ
        var updateRequest = new
        {
            Email = "profile@test.com",
            QrzUsername = "myqrzuser",
            QrzPassword = "myqrzpass123"
        };

        var response = await _client.PutAsJsonAsync("/api/auth/profile", updateRequest);

        // Assert - Utiliser Verify pour capturer le snapshot de la réponse de mise à jour du profil
        await Verify(response, _verifySettings)
            .UseParameters("ProfileUpdate");
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthentication();
        
        var updateRequest = new
        {
            Email = "test@example.com",
            QrzUsername = "testuser",
            QrzPassword = "testpass"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/profile", updateRequest);

        // Assert - Utiliser Verify pour capturer le snapshot de l'erreur d'autorisation
        await Verify(response, _verifySettings)
            .UseParameters("Unauthorized");
    }
}
