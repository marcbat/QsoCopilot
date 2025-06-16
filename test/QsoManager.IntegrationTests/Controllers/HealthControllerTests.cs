using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class HealthControllerTests : BaseIntegrationTest
{
    public HealthControllerTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    #region Health Check Tests (GET /Health)

    [Fact]
    public async Task Get_ShouldReturnHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/Health");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Get_ShouldReturnOkStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/Health");

        // Assert
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_MultipleRequests_ShouldAlwaysReturnHealthy()
    {
        // Act & Assert - Faire plusieurs requêtes pour vérifier la cohérence
        for (int i = 0; i < 3; i++)
        {
            var response = await _client.GetAsync("/Health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    #endregion
}
