using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

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
}
