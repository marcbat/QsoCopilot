using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Text.Json;

namespace QsoManager.IntegrationTests;

public class QsoAggregateTests : BaseIntegrationTest
{
    public QsoAggregateTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateQsoAggregate_WhenValidRequest_ShouldReturnCreatedQso()
    {
        // Arrange
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test Integration",
            Description = "QSO créé pour les tests d'intégration",
            ModeratorId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }   
    
    [Fact]
    public async Task CreateQsoAggregate_WithoutId_ShouldGenerateIdAndCreateQso()
    {
        // Arrange
        var createRequest = new
        {
            Name = "QSO Sans ID",
            Description = "QSO créé sans ID spécifique",
            ModeratorId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task CreateQsoAggregate_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var createRequest1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test",
            Description = "Premier QSO",
            ModeratorId = moderatorId
        };

        var createRequest2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test", // Même nom
            Description = "Deuxième QSO avec le même nom",
            ModeratorId = moderatorId
        };

        // Act
        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest1);
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest2);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task CreateQsoAggregate_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "", // Nom vide
            Description = "Description valide",
            ModeratorId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task CreateQsoAggregateAndAddParticipant_WhenValid_ShouldSucceed()
    {
        // Arrange
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO avec Participants",
            Description = "QSO pour tester l'ajout de participants",
            ModeratorId = Guid.NewGuid()
        };

        // Act - Créer le QSO
        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        
        // Extraire l'ID du QSO créé
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(createResponseContent);
        var qsoId = jsonDoc.RootElement.GetProperty("id").GetGuid();

        // Ajouter un participant - l'endpoint retourne maintenant le QSO complet
        var addParticipantRequest = new
        {
            CallSign = "F1ABC"
        };

        var addParticipantResponse = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Assert - Vérifier que l'ajout du participant retourne le QSO complet avec la liste des participants
        await Verify(addParticipantResponse, _verifySettings);
    }
}
