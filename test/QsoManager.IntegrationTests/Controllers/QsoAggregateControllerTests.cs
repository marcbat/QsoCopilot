using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers;

public class QsoAggregateControllerTests : BaseIntegrationTest
{
    public QsoAggregateControllerTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    #region Create Tests (POST /api/QsoAggregate)

    [Fact]
    public async Task Create_WhenValidRequest_ShouldReturnCreatedQso()
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
    public async Task Create_WithoutId_ShouldGenerateIdAndCreateQso()
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
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
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
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
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
    }

    #endregion

    #region GetAll Tests (GET /api/QsoAggregate)

    [Fact]
    public async Task GetAll_WhenNoQsoAggregates_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAll_WhenQsoAggregatesExist_ShouldReturnAllQsos()
    {
        // Arrange - Créer plusieurs QSO
        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test 1",
            Description = "Premier QSO pour test GetAll",
            ModeratorId = Guid.NewGuid()
        };

        var qso2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test 2",
            Description = "Deuxième QSO pour test GetAll",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate");

        // Assert
        await Verify(response, _verifySettings);
    }

    #endregion

    #region GetById Tests (GET /api/QsoAggregate/{id})

    [Fact]
    public async Task GetById_WhenQsoExists_ShouldReturnQso()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test GetById",
            Description = "QSO pour test GetById",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100); // Attendre que les projections soient mises à jour

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetById_WhenQsoNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/{nonExistentId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetById_WhenInvalidGuid_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/invalid-guid");

        // Assert
        await Verify(response, _verifySettings);
    }

    #endregion

    #region Search Tests (GET /api/QsoAggregate/search)

    [Fact]
    public async Task SearchByName_WhenNameExists_ShouldReturnMatchingQsos()
    {
        // Arrange
        var searchTerm = "Recherche Test";
        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = $"{searchTerm} QSO 1",
            Description = "Premier QSO pour recherche",
            ModeratorId = Guid.NewGuid()
        };

        var qso2 = new
        {
            Id = Guid.NewGuid(),
            Name = $"{searchTerm} QSO 2",
            Description = "Deuxième QSO pour recherche",
            ModeratorId = Guid.NewGuid()
        };

        var qso3 = new
        {
            Id = Guid.NewGuid(),
            Name = "Autre QSO",
            Description = "QSO qui ne correspond pas à la recherche",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso3);
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/search?name={searchTerm}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WhenNameIsEmpty_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search?name=");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WhenNoNameParameter_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search");

        // Assert
        await Verify(response, _verifySettings);
    }

    #endregion

    #region AddParticipant Tests (POST /api/QsoAggregate/{aggregateId}/participants)

    [Fact]
    public async Task AddParticipant_WhenValidRequest_ShouldAddParticipant()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Participants",
            Description = "QSO pour test des participants",
            ModeratorId = Guid.NewGuid()
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

    #endregion

    #region RemoveParticipant Tests (DELETE /api/QsoAggregate/{aggregateId}/participants/{callSign})

    [Fact]
    public async Task RemoveParticipant_WhenParticipantExists_ShouldRemoveParticipant()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Remove Participant",
            Description = "QSO pour test de suppression de participant",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        var callSign = "F4DEF";
        var addParticipantRequest = new { CallSign = callSign };
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);
        await Task.Delay(100);

        // Act
        var response = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}/participants/{callSign}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task RemoveParticipant_WhenParticipantNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Remove Non-Existent Participant",
            Description = "QSO pour test de suppression de participant inexistant",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Act
        var response = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}/participants/F4INEXISTANT");

        // Assert
        await Verify(response, _verifySettings);
    }

    #endregion

    #region ReorderParticipants Tests (PUT /api/QsoAggregate/{aggregateId}/participants/reorder)

    [Fact]
    public async Task ReorderParticipants_WhenValidRequest_ShouldReorderParticipants()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Reorder",
            Description = "QSO pour test de réorganisation",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter plusieurs participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4CCC" });
        await Task.Delay(100);

        var reorderRequest = new
        {
            NewOrders = new Dictionary<string, int>
            {
                { "F4AAA", 2 },
                { "F4BBB", 0 },
                { "F4CCC", 1 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/reorder", reorderRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    #endregion

    #region MoveParticipant Tests (PUT /api/QsoAggregate/{aggregateId}/participants/{callSign}/move)

    [Fact]
    public async Task MoveParticipant_WhenValidRequest_ShouldMoveParticipant()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Move Participant",
            Description = "QSO pour test de déplacement de participant",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter plusieurs participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4CCC" });
        await Task.Delay(100);

        var moveRequest = new
        {
            NewPosition = 0
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/F4CCC/move", moveRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task MoveParticipant_WhenParticipantNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Move Non-Existent Participant",
            Description = "QSO pour test de déplacement de participant inexistant",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        var moveRequest = new
        {
            NewPosition = 0
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/F4INEXISTANT/move", moveRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    #endregion
}
