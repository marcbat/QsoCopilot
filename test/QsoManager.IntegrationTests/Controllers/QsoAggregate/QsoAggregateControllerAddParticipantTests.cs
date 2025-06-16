using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerAddParticipantTests : BaseIntegrationTest
{
    public QsoAggregateControllerAddParticipantTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }    [Fact]
    public async Task AddParticipant_WhenValidRequest_ShouldAddParticipant()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Participants",
            Description = "QSO pour test des participants"
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        var addParticipantRequest = new
        {
            CallSign = "F4ABC"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task AddParticipant_WhenQsoNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentQsoId = Guid.NewGuid();
        var addParticipantRequest = new
        {
            CallSign = "F4XYZ"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/QsoAggregate/{nonExistentQsoId}/participants", addParticipantRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task AddParticipant_WhenNotModerator_ShouldReturnForbidden()
    {
        // Arrange - Créer un QSO avec un premier utilisateur
        var (moderatorId, moderatorToken) = await CreateAndAuthenticateUserAsync("F4MODERATOR");
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Unauthorized",
            Description = "QSO pour test d'autorisation"
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Changer d'utilisateur - utiliser un autre utilisateur qui n'est pas le modérateur
        var (otherId, otherToken) = await CreateAndAuthenticateUserAsync("F4OTHER");
        
        var addParticipantRequest = new
        {
            CallSign = "F4XYZ"
        };

        // Act - Essayer d'ajouter un participant avec un utilisateur différent
        var response = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task AddParticipant_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange - Créer un QSO avec un utilisateur authentifié
        var (moderatorId, moderatorToken) = await CreateAndAuthenticateUserAsync("F4MODERATOR2");
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test No Auth",
            Description = "QSO pour test sans authentification"
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Supprimer l'authentification
        ClearAuthentication();
        
        var addParticipantRequest = new
        {
            CallSign = "F4NOAUTH"
        };

        // Act - Essayer d'ajouter un participant sans authentification
        var response = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
