using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

public class QsoAggregateControllerReorderParticipantsTests : BaseIntegrationTest
{
    public QsoAggregateControllerReorderParticipantsTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task ReorderParticipants_WhenValidRequest_ShouldReorderParticipants()
    {
        // Arrange
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Reorder",
            Description = "QSO pour test de réorganisation",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(100);

        // Ajouter plusieurs participants
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4AAA" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4BBB" });
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", new { CallSign = "F4CCC" });
        await Task.Delay(100);

        var reorderRequest = new
        {
            NewOrders = new Dictionary<string, int>
            {
                { "F4AAA", 2 },
                { "F4BBB", 0 },
                { "F4CCC", 1 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants/reorder", reorderRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
