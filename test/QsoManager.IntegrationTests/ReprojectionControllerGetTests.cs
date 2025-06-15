using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net;

namespace QsoManager.IntegrationTests;

public class ReprojectionControllerGetTests : BaseIntegrationTest
{
    public ReprojectionControllerGetTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task GetAllReprojectionStatuses_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/Reprojection/status");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetReprojectionStatus_WhenTaskNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Reprojection/status/{nonExistentTaskId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetReprojectionStatus_WhenInvalidGuid_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/Reprojection/status/invalid-guid");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetReprojectionStatus_AfterStartingReprojection_ShouldReturnStatus()
    {
        // Arrange - Démarrer une reprojection
        var startResponse = await _client.PostAsync("/api/Reprojection/start", null);
        startResponse.EnsureSuccessStatusCode();
        
        var content = await startResponse.Content.ReadAsStringAsync();
        var taskResponse = System.Text.Json.JsonSerializer.Deserialize<ReprojectionTaskResponse>(content, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act
        var response = await _client.GetAsync($"/api/Reprojection/status/{taskResponse!.TaskId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAllReprojectionStatuses_AfterStartingReprojection_ShouldReturnStatuses()
    {
        // Arrange - Démarrer une reprojection
        await _client.PostAsync("/api/Reprojection/start", null);

        // Act
        var response = await _client.GetAsync("/api/Reprojection/status");

        // Assert
        await Verify(response, _verifySettings);
    }
}

// DTO pour désérialiser la réponse
public record ReprojectionTaskResponse
{
    public Guid TaskId { get; init; }
}
