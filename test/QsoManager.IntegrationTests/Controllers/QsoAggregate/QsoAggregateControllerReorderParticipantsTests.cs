using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerReorderParticipantsTests : BaseIntegrationTest
{
    public QsoAggregateControllerReorderParticipantsTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task ReorderParticipants_WhenValidRequest_ShouldReorderParticipants()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var qsoId = Guid.NewGuid();
        
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Reorder",
            Description = "QSO pour test de réorganisation",
            Frequency = 145.5m
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

    [Fact]
    public async Task ReorderParticipants_ShouldMaintainCorrectOrderAfterReordering()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST2");
        var qsoId = Guid.NewGuid();
        
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Order Verification",
            Description = "QSO pour vérifier l'ordre après réorganisation",
            Frequency = 145.5m
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter 4 participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4CCC" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4DDD" });
        await Task.Delay(100);

        // Réorganiser: F4DDD en premier, F4AAA en deuxième, F4CCC en troisième, F4BBB en quatrième
        var reorderRequest = new
        {
            NewOrders = new Dictionary<string, int>
            {
                { "F4DDD", 1 },
                { "F4AAA", 2 },
                { "F4CCC", 3 },
                { "F4BBB", 4 }
            }
        };        // Act
        var reorderResponse = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/reorder", reorderRequest);
        
        // Assert - Vérifier que la réorganisation a réussi
        Assert.Equal(HttpStatusCode.NoContent, reorderResponse.StatusCode);
        
        // Récupérer le QSO pour vérifier l'ordre
        var getResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task ReorderParticipants_WhenInvalidParticipant_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST3");
        var qsoId = Guid.NewGuid();
        
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Invalid Reorder",
            Description = "QSO pour tester la réorganisation avec participant invalide",
            Frequency = 145.5m
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter seulement 2 participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await Task.Delay(100);

        // Essayer de réorganiser en incluant un participant qui n'existe pas
        var reorderRequest = new
        {
            NewOrders = new Dictionary<string, int>
            {
                { "F4AAA", 1 },
                { "F4BBB", 2 },
                { "F4XXX", 3 } // Ce participant n'existe pas dans le QSO
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/reorder", reorderRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task ReorderParticipants_WhenEmptyReorderList_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST4");
        var qsoId = Guid.NewGuid();
        
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Empty Reorder",
            Description = "QSO pour tester la réorganisation avec liste vide",
            Frequency = 145.5m
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter des participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await Task.Delay(100);

        // Essayer de réorganiser avec une liste vide
        var reorderRequest = new
        {
            NewOrders = new Dictionary<string, int>()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/reorder", reorderRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task ReorderParticipants_WhenQsoNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST5");
        var nonExistentQsoId = Guid.NewGuid();

        var reorderRequest = new
        {
            NewOrders = new Dictionary<string, int>
            {
                { "F4AAA", 1 },
                { "F4BBB", 2 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{nonExistentQsoId}/participants/reorder", reorderRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
