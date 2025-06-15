using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

public class QsoAggregateControllerMoveParticipantTests : BaseIntegrationTest
{
    public QsoAggregateControllerMoveParticipantTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task MoveParticipant_WhenValidRequest_ShouldMoveParticipant()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Move Participant",
            Description = "QSO pour test de déplacement de participant",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter plusieurs participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4CCC" });
        await Task.Delay(100);

        var moveRequest = new
        {
            NewPosition = 0
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/F4CCC/move", moveRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task MoveParticipant_WhenParticipantNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Move Non-Existent Participant",
            Description = "QSO pour test de déplacement de participant inexistant",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        var moveRequest = new
        {
            NewPosition = 0
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/F4INEXISTANT/move", moveRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
