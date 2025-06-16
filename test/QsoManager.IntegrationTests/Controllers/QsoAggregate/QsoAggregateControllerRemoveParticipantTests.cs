using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerRemoveParticipantTests : BaseIntegrationTest
{
    public QsoAggregateControllerRemoveParticipantTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }    [Fact]
    public async Task RemoveParticipant_WhenParticipantExists_ShouldRemoveParticipant()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST1");
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Remove Participant",
            Description = "QSO pour test de suppression de participant"
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        var callSign = "F4DEF";
        var addParticipantRequest = new { CallSign = callSign };
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);
        await Task.Delay(100);

        // Act
        var response = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}/participants/{callSign}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task RemoveParticipant_WhenParticipantNotFound_ShouldReturnBadRequest()
    {        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST2");
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Remove Non-Existent Participant",
            Description = "QSO pour test de suppression de participant inexistant"
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Act
        var response = await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}/participants/F4INEXISTANT");

        // Assert
        await Verify(response, _verifySettings);
    }
}
