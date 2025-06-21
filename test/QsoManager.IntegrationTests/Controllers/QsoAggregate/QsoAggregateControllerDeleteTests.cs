using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerDeleteTests : BaseIntegrationTest
{
    public QsoAggregateControllerDeleteTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task Delete_WhenAuthenticatedAsModerator_ShouldDeleteQsoSuccessfully()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var qsoId = Guid.NewGuid();

        // Créer un QSO d'abord
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Delete",
            Description = "QSO créé pour test de suppression",
            Frequency = 14.205m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act - Supprimer le QSO
        var deleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(deleteResponse, _verifySettings);
    }

    [Fact]
    public async Task Delete_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        ClearAuthentication(); // S'assurer qu'il n'y a pas d'authentification

        // Act
        var response = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Delete_WhenQsoNotExists_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST2");
        var nonExistentQsoId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/QsoAggregate/{nonExistentQsoId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Delete_WhenNotModerator_ShouldReturnBadRequest()
    {
        // Arrange
        // Créer le modérateur qui va créer le QSO
        var (moderatorId, moderatorToken) = await CreateAndAuthenticateUserAsync("F4MODERATOR");
        var qsoId = Guid.NewGuid();

        // Créer un QSO avec le modérateur
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Delete Permission",
            Description = "QSO pour test de permissions de suppression",
            Frequency = 7.144m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Créer un autre utilisateur (non modérateur de ce QSO)
        var (otherUserId, otherToken) = await CreateAndAuthenticateUserAsync("F4OTHER");

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act - Essayer de supprimer le QSO avec l'autre utilisateur
        var deleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(deleteResponse, _verifySettings);
    }

    [Fact]
    public async Task Delete_WhenInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST3");

        // Act
        var response = await _client.DeleteAsync("/api/QsoAggregate/invalid-guid");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Delete_WhenQsoAlreadyDeleted_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST4");
        var qsoId = Guid.NewGuid();

        // Créer un QSO
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Double Delete",
            Description = "QSO pour test de double suppression",
            Frequency = 21.205m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Supprimer le QSO une première fois
        var firstDeleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");
        firstDeleteResponse.EnsureSuccessStatusCode();

        // Attendre que les événements soient traités
        await Task.Delay(100);

        // Act - Essayer de supprimer le QSO une deuxième fois
        var secondDeleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(secondDeleteResponse, _verifySettings);
    }

    [Fact]
    public async Task Delete_ShouldRemoveQsoFromGetAllResults()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST5");
        var qsoId = Guid.NewGuid();

        // Créer un QSO
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Visibility After Delete",
            Description = "QSO pour vérifier qu'il disparaît après suppression",
            Frequency = 28.405m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Vérifier que le QSO existe dans la liste
        ClearAuthentication(); // GetAll est public
        var getAllBeforeResponse = await _client.GetAsync("/api/QsoAggregate");
        getAllBeforeResponse.EnsureSuccessStatusCode();

        // Remettre l'authentification pour supprimer
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Supprimer le QSO
        var deleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");
        deleteResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(200);

        // Vérifier que le QSO n'existe plus dans la liste
        ClearAuthentication();
        var getAllAfterResponse = await _client.GetAsync("/api/QsoAggregate");

        // Assert - Vérifier les deux réponses
        await Verify(new { 
            BeforeDelete = getAllBeforeResponse, 
            AfterDelete = getAllAfterResponse,
            DeleteResponse = deleteResponse
        }, _verifySettings);
    }

    [Fact]
    public async Task Delete_ShouldMakeQsoNotFoundOnGetById()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST6");
        var qsoId = Guid.NewGuid();

        // Créer un QSO
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test GetById After Delete",
            Description = "QSO pour vérifier GetById après suppression",
            Frequency = 145.625m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Supprimer le QSO
        var deleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}");
        deleteResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(200);

        // Act - Essayer de récupérer le QSO supprimé
        ClearAuthentication(); // GetById est public
        var getByIdResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(new { 
            DeleteResponse = deleteResponse,
            GetByIdAfterDelete = getByIdResponse
        }, _verifySettings);
    }
}
