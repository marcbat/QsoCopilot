using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerGetAllTests : BaseIntegrationTest
{
    public QsoAggregateControllerGetAllTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task GetAll_WhenNoQsoAggregates_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate");

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task GetAll_WhenQsoAggregatesExist_ShouldReturnAllQsos()
    {
        // Arrange - Créer plusieurs QSO avec utilisateurs authentifiés
        var (userId1, token1) = await CreateAndAuthenticateUserAsync("F4TEST1");        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test 1",
            Description = "Premier QSO pour test GetAll",
            Frequency = 14.205m
        };
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);

        // Changer d'utilisateur pour le deuxième QSO
        var (userId2, token2) = await CreateAndAuthenticateUserAsync("F4TEST2");
        var qso2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test 2",
            Description = "Deuxième QSO pour test GetAll",
            Frequency = 7.040m
        };
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);

        // Supprimer l'authentification pour le test GetAll (lecture publique)
        ClearAuthentication();

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate");

        // Assert
        await Verify(response, _verifySettings);
    }
}
