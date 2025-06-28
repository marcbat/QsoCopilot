using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Xunit;
using QsoManager.Application.DTOs;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerHistoryTests : BaseIntegrationTest
{
    public QsoAggregateControllerHistoryTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task GetById_AfterCreation_ShouldContainHistoryEntries()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4HIST1");
        var qsoId = Guid.NewGuid();

        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Historique",
            Description = "QSO pour test historique",
            Frequency = 145.800m
        };

        // Act - Créer le QSO
        var createResponse = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Attendre que les projections soient mises à jour
        await Task.Delay(500);

        // Supprimer l'authentification pour le test GetById (lecture publique)
        ClearAuthentication();

        // Act - Récupérer le QSO
        var getResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");
        getResponse.EnsureSuccessStatusCode();

        var content = await getResponse.Content.ReadAsStringAsync();
        var qso = JsonSerializer.Deserialize<QsoAggregateDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert - Vérifier que l'historique existe et contient des entrées
        Assert.NotNull(qso);
        Assert.NotNull(qso.History);
        Assert.NotEmpty(qso.History);

        // Vérifier qu'il y a au moins 2 entrées : création du QSO + ajout du modérateur comme participant
        Assert.True(qso.History.Count >= 2, $"L'historique devrait contenir au moins 2 entrées, mais en contient {qso.History.Count}");

        // Vérifier que l'historique contient des messages appropriés
        var historyMessages = qso.History.Values.ToList();
        Assert.Contains(historyMessages, msg => msg.Contains("Création du QSO"));
        Assert.Contains(historyMessages, msg => msg.Contains("Ajout du participant F4HIST1"));

        // Log pour debugging
        Console.WriteLine($"Historique trouvé avec {qso.History.Count} entrées:");
        foreach (var entry in qso.History.OrderBy(h => h.Key))
        {
            Console.WriteLine($"- {entry.Key}: {entry.Value}");
        }
    }

    [Fact]
    public async Task GetById_AfterParticipantOperations_ShouldTrackAllChanges()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4HIST2");
        var qsoId = Guid.NewGuid();

        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Historique Complet",
            Description = "QSO pour test historique complet",
            Frequency = 145.800m
        };

        // Act - Créer le QSO
        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(200);

        // Ajouter un participant
        var addParticipantRequest = new { CallSign = "F1TEST" };
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", addParticipantRequest);
        await Task.Delay(200);

        // Supprimer un participant
        await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}/participants/F1TEST");
        await Task.Delay(200);

        // Supprimer l'authentification pour le test GetById
        ClearAuthentication();

        // Récupérer le QSO
        var getResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");
        getResponse.EnsureSuccessStatusCode();

        var content = await getResponse.Content.ReadAsStringAsync();
        var qso = JsonSerializer.Deserialize<QsoAggregateDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(qso);
        Assert.NotNull(qso.History);
        Assert.True(qso.History.Count >= 4, $"L'historique devrait contenir au moins 4 entrées, mais en contient {qso.History.Count}");

        var historyMessages = qso.History.Values.ToList();
        Assert.Contains(historyMessages, msg => msg.Contains("Création du QSO"));
        Assert.Contains(historyMessages, msg => msg.Contains("Ajout du participant F4HIST2"));
        Assert.Contains(historyMessages, msg => msg.Contains("Ajout du participant F1TEST"));
        Assert.Contains(historyMessages, msg => msg.Contains("Suppression du participant F1TEST"));

        // Log pour debugging
        Console.WriteLine($"Historique complet trouvé avec {qso.History.Count} entrées:");
        foreach (var entry in qso.History.OrderBy(h => h.Key))
        {
            Console.WriteLine($"- {entry.Key}: {entry.Value}");
        }
    }

    [Fact]
    public async Task GetHistory_ShouldReturnHistoryOnly()
    {
        // Arrange
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4HIST3");
        var qsoId = Guid.NewGuid();

        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test Endpoint Historique",
            Description = "QSO pour test endpoint historique",
            Frequency = 145.800m
        };

        // Act - Créer le QSO
        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);
        await Task.Delay(500);

        // Supprimer l'authentification
        ClearAuthentication();

        // Récupérer l'historique via le endpoint spécifique
        var historyResponse = await _client.GetAsync($"/api/QsoAggregate/{qsoId}/history");
        historyResponse.EnsureSuccessStatusCode();

        var historyContent = await historyResponse.Content.ReadAsStringAsync();
        var history = JsonSerializer.Deserialize<Dictionary<DateTime, string>>(historyContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(history);
        Assert.NotEmpty(history);
        Assert.True(history.Count >= 2, $"L'historique devrait contenir au moins 2 entrées, mais en contient {history.Count}");

        var historyMessages = history.Values.ToList();
        Assert.Contains(historyMessages, msg => msg.Contains("Création du QSO"));
        Assert.Contains(historyMessages, msg => msg.Contains("Ajout du participant F4HIST3"));

        // Log pour debugging
        Console.WriteLine($"Historique via endpoint spécifique trouvé avec {history.Count} entrées:");
        foreach (var entry in history.OrderBy(h => h.Key))
        {
            Console.WriteLine($"- {entry.Key}: {entry.Value}");
        }
    }
}
