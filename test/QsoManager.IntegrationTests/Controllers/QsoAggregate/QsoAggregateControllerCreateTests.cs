using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

public class QsoAggregateControllerCreateTests : BaseIntegrationTest
{
    public QsoAggregateControllerCreateTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }    [Fact]
    public async Task Create_WhenValidRequest_ShouldReturnCreatedQso()
    {
        // Arrange
        var moderatorId = await CreateValidModeratorAsync("F4TEST1");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test Integration",
            Description = "QSO créé pour les tests d'intégration",
            ModeratorId = moderatorId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task Create_WithoutId_ShouldGenerateIdAndCreateQso()
    {
        // Arrange
        var moderatorId = await CreateValidModeratorAsync("F4TEST2");
        var createRequest = new
        {
            Name = "QSO Sans ID",
            Description = "QSO créé sans ID spécifique",
            ModeratorId = moderatorId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var moderatorId = await CreateValidModeratorAsync("F4TEST3");
        var createRequest1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test",
            Description = "Premier QSO",
            ModeratorId = moderatorId
        };

        var createRequest2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Duplicate Test", // Même nom
            Description = "Deuxième QSO avec le même nom",
            ModeratorId = moderatorId
        };

        // Act
        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest1);
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest2);

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var moderatorId = await CreateValidModeratorAsync("F4TEST4");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "", // Nom vide
            Description = "Description valide",
            ModeratorId = moderatorId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithNonExistentModerator_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentModeratorId = Guid.NewGuid(); // Un ID qui n'existe pas dans la base
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO avec modérateur inexistant",
            Description = "QSO créé avec un ModeratorId qui n'existe pas",
            ModeratorId = nonExistentModeratorId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Create_WithValidModerator_ShouldReturnCreatedQso()
    {
        // Arrange - Créer d'abord un utilisateur (qui créera automatiquement un modérateur)
        var moderatorId = await CreateValidModeratorAsync("F4ABC");

        // Créer le QSO avec le modérateur valide
        var createQsoRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO avec modérateur valide",
            Description = "QSO créé avec un ModeratorId qui existe",
            ModeratorId = moderatorId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createQsoRequest);        // Assert
        await Verify(response, _verifySettings);
    }

    private async Task<Guid> CreateValidModeratorAsync(string callSign = "F4TEST")
    {
        var registerRequest = new
        {
            UserName = callSign,
            Password = "Test123!@#",
            Email = $"{callSign.ToLower()}@example.com"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        using var document = System.Text.Json.JsonDocument.Parse(content);
        var userIdString = document.RootElement.GetProperty("userId").GetString();
        
        // L'ID retourné par register est l'ID de l'utilisateur, qui est aussi l'ID du modérateur
        return Guid.Parse(userIdString!);
    }
}
