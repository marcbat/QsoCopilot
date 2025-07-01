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
        var qsoId = Guid.NewGuid();        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Participants",
            Description = "QSO pour test des participants",
            Frequency = 145.5m
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
    public async Task AddParticipant_ShouldUpdateProjectionWithParticipant()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST_PROJECTION");
        var qsoId = Guid.NewGuid();        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Projection",
            Description = "QSO pour test de la projection",
            Frequency = 14.205m
        };

        // Créer le QSO
        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        Assert.True(createResponse.IsSuccessStatusCode, "QSO creation should succeed");
        
        // Attendre que la projection initiale soit créée
        await Task.Delay(200);        // Vérifier l'état initial du QSO (le modérateur est automatiquement ajouté comme participant)
        var initialGetResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");
        Assert.True(initialGetResponse.IsSuccessStatusCode, "Initial GET should succeed");
        
        var initialContent = await initialGetResponse.Content.ReadAsStringAsync();
        var initialQso = JsonSerializer.Deserialize<JsonElement>(initialContent);
        var initialParticipants = initialQso.GetProperty("participants");
        Assert.Equal(1, initialParticipants.GetArrayLength()); // Le modérateur est automatiquement ajouté
        Assert.Equal("F4TEST_PROJECTION", initialParticipants[0].GetProperty("callSign").GetString());

        // Ajouter un participant
        var addParticipantRequest = new
        {
            CallSign = "F4PARTICIPANT1"
        };

        var addResponse = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);
        Assert.True(addResponse.IsSuccessStatusCode, "Add participant should succeed");

        // Attendre que la projection soit mise à jour
        await Task.Delay(300);

        // Vérifier que le participant est bien dans la projection
        var updatedGetResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");
        Assert.True(updatedGetResponse.IsSuccessStatusCode, "Updated GET should succeed");
        
        var updatedContent = await updatedGetResponse.Content.ReadAsStringAsync();
        var updatedQso = JsonSerializer.Deserialize<JsonElement>(updatedContent);
        var updatedParticipants = updatedQso.GetProperty("participants");
          // Assert - Le participant doit être présent dans la projection (+ le modérateur)
        Assert.Equal(2, updatedParticipants.GetArrayLength()); // Modérateur + nouveau participant
        
        // Vérifier que le modérateur est toujours présent
        var moderatorParticipant = updatedParticipants.EnumerateArray()
            .FirstOrDefault(p => p.GetProperty("callSign").GetString() == "F4TEST_PROJECTION");
        Assert.True(moderatorParticipant.ValueKind != JsonValueKind.Undefined, "Moderator should still be present");
        Assert.Equal(1, moderatorParticipant.GetProperty("order").GetInt32());
        
        // Vérifier que le nouveau participant est présent
        var newParticipant = updatedParticipants.EnumerateArray()
            .FirstOrDefault(p => p.GetProperty("callSign").GetString() == "F4PARTICIPANT1");
        Assert.True(newParticipant.ValueKind != JsonValueKind.Undefined, "New participant should be present");
        Assert.Equal(2, newParticipant.GetProperty("order").GetInt32());
    }

    [Fact]
    public async Task AddMultipleParticipants_ShouldMaintainCorrectOrder()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST_MULTIPLE");
        var qsoId = Guid.NewGuid();        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Multiple Participants",
            Description = "QSO pour test de plusieurs participants",
            Frequency = 7.040m
        };

        // Créer le QSO
        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(200);

        // Ajouter le premier participant
        var addParticipant1 = new { CallSign = "F4FIRST" };
        var response1 = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipant1);
        Assert.True(response1.IsSuccessStatusCode);
        await Task.Delay(300);

        // Ajouter le deuxième participant
        var addParticipant2 = new { CallSign = "F4SECOND" };
        var response2 = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipant2);
        Assert.True(response2.IsSuccessStatusCode);
        await Task.Delay(300);

        // Vérifier l'état final
        var getResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");
        Assert.True(getResponse.IsSuccessStatusCode);
        
        var content = await getResponse.Content.ReadAsStringAsync();
        var qso = JsonSerializer.Deserialize<JsonElement>(content);
        var participants = qso.GetProperty("participants");
          // Assert - Trois participants avec les bons ordres (modérateur + 2 ajoutés)
        Assert.Equal(3, participants.GetArrayLength());
        
        // Vérifier le modérateur (premier participant automatique)
        var moderatorParticipant = participants.EnumerateArray()
            .FirstOrDefault(p => p.GetProperty("callSign").GetString() == "F4TEST_MULTIPLE");
        Assert.True(moderatorParticipant.ValueKind != JsonValueKind.Undefined, "Moderator should be present");
        Assert.Equal(1, moderatorParticipant.GetProperty("order").GetInt32());
        
        // Vérifier les participants ajoutés manuellement
        var firstParticipant = participants.EnumerateArray()
            .FirstOrDefault(p => p.GetProperty("callSign").GetString() == "F4FIRST");
        var secondParticipant = participants.EnumerateArray()
            .FirstOrDefault(p => p.GetProperty("callSign").GetString() == "F4SECOND");
        
        Assert.True(firstParticipant.ValueKind != JsonValueKind.Undefined, "F4FIRST should be present");
        Assert.True(secondParticipant.ValueKind != JsonValueKind.Undefined, "F4SECOND should be present");
        Assert.Equal(2, firstParticipant.GetProperty("order").GetInt32());
        Assert.Equal(3, secondParticipant.GetProperty("order").GetInt32());
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
        var qsoId = Guid.NewGuid();        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Unauthorized",
            Description = "QSO pour test d'autorisation",
            Frequency = 28.400m
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
        var qsoId = Guid.NewGuid();        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test No Auth",
            Description = "QSO pour test sans authentification",
            Frequency = 21.205m
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
