using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerSearchTests : BaseIntegrationTest
{
    public QsoAggregateControllerSearchTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }    [Fact]
    public async Task SearchByName_WhenNameExists_ShouldReturnMatchingQsos()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var searchTerm = "Recherche Test";        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = $"{searchTerm} QSO 1",
            Description = "Premier QSO pour recherche",
            Frequency = 14.205m
        };

        var qso2 = new
        {
            Id = Guid.NewGuid(),
            Name = $"{searchTerm} QSO 2",
            Description = "Deuxième QSO pour recherche",
            Frequency = 7.040m
        };

        var qso3 = new
        {
            Id = Guid.NewGuid(),
            Name = "Autre QSO",
            Description = "QSO qui ne correspond pas à la recherche",
            Frequency = 21.205m
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso3);
        await Task.Delay(100);

        // Supprimer l'authentification pour le test de recherche (lecture publique)
        ClearAuthentication();

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/search?name={searchTerm}");

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
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
    }    [Fact]
    public async Task SearchByName_PartialMatch_ShouldReturnMatchingQsos()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST2");
        
        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = "Mon Super QSO 2",
            Description = "QSO avec Super dans le nom",
            Frequency = 14.205m
        };

        var qso2 = new
        {
            Id = Guid.NewGuid(),
            Name = "Un autre QSO super",
            Description = "Encore un QSO avec super",
            Frequency = 7.040m
        };

        var qso3 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO normal",
            Description = "QSO sans le mot recherché",
            Frequency = 21.205m
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso3);
        
        // Attendre plus longtemps pour que les projections soient mises à jour
        await Task.Delay(500);

        // Supprimer l'authentification pour le test de recherche (lecture publique)
        ClearAuthentication();

        // Vérifier d'abord que tous les QSO ont été créés
        var getAllResponse = await _client.GetAsync("/api/QsoAggregate");
        
        // Act - Test avec différentes variations de casse
        var responseSuper = await _client.GetAsync("/api/QsoAggregate/search?name=super");
        var responseSuper2 = await _client.GetAsync("/api/QsoAggregate/search?name=Super");
        var responseSuPer = await _client.GetAsync("/api/QsoAggregate/search?name=SuPer");

        // Assert
        await Verify(new { getAllResponse, responseSuper, responseSuper2, responseSuPer }, _verifySettings);
    }
}
