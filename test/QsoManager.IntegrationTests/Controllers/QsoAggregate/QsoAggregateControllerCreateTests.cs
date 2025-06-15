using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

public class QsoAggregateControllerCreateTests : BaseIntegrationTest
{
    public QsoAggregateControllerCreateTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task Create_WhenAuthenticated_ShouldReturnCreatedQso()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test Integration",
            Description = "QSO créé pour les tests d'intégration"
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
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST2");
        var createRequest = new
        {
            Name = "QSO Sans ID",
            Description = "QSO créé sans ID spécifique"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthentication(); // S'assurer qu'il n'y a pas de token
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Non Autorisé",
            Description = "QSO créé sans authentification"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST3");
        var createRequest1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test",
            Description = "Premier QSO"
        };

        var createRequest2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test", // Même nom
            Description = "Deuxième QSO avec le même nom"
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
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST4");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "", // Nom vide
            Description = "Description valide"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithValidAuthentication_ShouldReturnCreatedQso()
    {
        // Arrange - Créer et authentifier un utilisateur
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4ABC");

        // Créer le QSO (le modérateur sera automatiquement celui de l'utilisateur authentifié)
        var createQsoRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO avec utilisateur authentifié",
            Description = "QSO créé avec un utilisateur authentifié"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createQsoRequest);        // Assert
        await Verify(response, _verifySettings);
    }
}
