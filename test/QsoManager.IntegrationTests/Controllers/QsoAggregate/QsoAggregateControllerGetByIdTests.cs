using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerGetByIdTests : BaseIntegrationTest
{
    public QsoAggregateControllerGetByIdTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }    [Fact]
    public async Task GetById_WhenQsoExists_ShouldReturnQso()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var qsoId = Guid.NewGuid();        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test GetById",
            Description = "QSO pour test GetById",
            Frequency = 145.800m
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100); // Attendre que les projections soient mises à jour

        // Supprimer l'authentification pour le test GetById (lecture publique)
        ClearAuthentication();

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
}
