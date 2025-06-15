using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

public class QsoAggregateControllerSearchTests : BaseIntegrationTest
{
    public QsoAggregateControllerSearchTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

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
}
