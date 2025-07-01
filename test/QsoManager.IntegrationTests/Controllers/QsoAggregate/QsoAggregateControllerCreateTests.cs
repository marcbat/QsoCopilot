using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerCreateTests : BaseIntegrationTest
{
    public QsoAggregateControllerCreateTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task Create_WhenAuthenticated_ShouldReturnCreatedQso()
    {        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test Integration",
            Description = "QSO créé pour les tests d'intégration",
            Frequency = 14.230m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithoutId_ShouldGenerateIdAndCreateQso()
    {        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST2");
        var createRequest = new
        {
            Name = "QSO Sans ID",
            Description = "QSO créé sans ID spécifique",
            Frequency = 7.144m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task Create_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthentication(); // S'assurer qu'il n'y a pas de token
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Non Autorisé",
            Description = "QSO créé sans authentification",
            Frequency = 21.200m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST3");
        var createRequest1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test",
            Description = "Premier QSO",
            Frequency = 28.400m
        };

        var createRequest2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test", // Même nom
            Description = "Deuxième QSO avec le même nom",
            Frequency = 28.450m
        };

        // Act
        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest1);
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest2);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST4");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "", // Nom vide
            Description = "Description valide",
            Frequency = 0m // Fréquence invalide
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithValidAuthentication_ShouldReturnCreatedQso()
    {        // Arrange - Créer et authentifier un utilisateur
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4ABC");

        // Créer le QSO (le modérateur sera automatiquement celui de l'utilisateur authentifié)
        var createQsoRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO avec utilisateur authentifié",
            Description = "QSO créé avec un utilisateur authentifié",
            Frequency = 145.500m
        };        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createQsoRequest);        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithoutFrequency_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST5");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Sans Fréquence",
            Description = "QSO créé sans fréquence"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithNegativeFrequency_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST6");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Fréquence Négative",
            Description = "QSO avec fréquence négative",
            Frequency = -14.230m
        };        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithSameNameAfterDelete_ShouldAllowCreation()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST7");
        var qsoName = "QSO Test Same Name After Delete";
        
        // Créer le premier QSO
        var createRequest1 = new
        {
            Id = Guid.NewGuid(),
            Name = qsoName,
            Description = "Premier QSO qui sera supprimé",
            Frequency = 145.500m
        };

        var firstCreateResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest1);
        firstCreateResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(200);

        // Supprimer le premier QSO
        var deleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{createRequest1.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        // Attendre que les événements de suppression soient traités
        await Task.Delay(200);

        // Créer un nouveau QSO avec le même nom
        var createRequest2 = new
        {
            Id = Guid.NewGuid(),
            Name = qsoName, // Même nom que le QSO supprimé
            Description = "Nouveau QSO avec le même nom",
            Frequency = 145.600m
        };

        // Act
        var secondCreateResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest2);

        // Assert - Doit réussir car le premier QSO a été supprimé
        await Verify(secondCreateResponse, _verifySettings);
    }
    
    [Fact]
    public async Task Create_WithNameOfDeletedQso_ShouldSucceed()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST7");
        var qsoName = "QSO Test Réutilisation Nom";
        
        // Créer le premier QSO
        var createRequest1 = new
        {
            Id = Guid.NewGuid(),
            Name = qsoName,
            Description = "Premier QSO qui sera supprimé",
            Frequency = 145.500m
        };

        var firstCreateResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest1);
        firstCreateResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(200);

        // Supprimer le premier QSO
        var deleteResponse = await _client.DeleteAsync($"/api/QsoAggregate/{createRequest1.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        // Attendre que les événements de suppression soient traités
        await Task.Delay(300);

        // Créer un nouveau QSO avec le même nom
        var createRequest2 = new
        {
            Id = Guid.NewGuid(),
            Name = qsoName, // Même nom que le QSO supprimé
            Description = "Nouveau QSO avec le même nom",
            Frequency = 145.600m
        };

        // Act
        var secondCreateResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest2);

        // Assert - Doit réussir car le premier QSO a été supprimé
        await Verify(secondCreateResponse, _verifySettings);
    }
}
