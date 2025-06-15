using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

public class QsoAggregateControllerAddParticipantTests : BaseIntegrationTest
{
    public QsoAggregateControllerAddParticipantTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task AddParticipant_WhenValidRequest_ShouldAddParticipant()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Participants",
            Description = "QSO pour test des participants",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        var addParticipantRequest = new
        {
            CallSign = "F4ABC"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task AddParticipant_WhenQsoNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentQsoId = Guid.NewGuid();
        var addParticipantRequest = new
        {
            CallSign = "F4XYZ"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/QsoAggregate/{nonExistentQsoId}/participants", addParticipantRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
