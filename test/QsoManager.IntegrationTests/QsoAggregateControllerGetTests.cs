using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;

namespace QsoManager.IntegrationTests;

public class QsoAggregateControllerGetTests : BaseIntegrationTest
{
    public QsoAggregateControllerGetTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
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
        };        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate");

        // Assert
        await Verify(response, _verifySettings);
    }    
    
    [Fact]
    public async Task GetById_WhenQsoExists_ShouldReturnQso()
    {
        // Arrange - Créer un QSO
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO Test GetById",
            Description = "QSO pour tester GetById",
            ModeratorId = Guid.NewGuid()
        };        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(response, _verifySettings);
    }    
    
    [Fact]
    public async Task GetById_WhenQsoNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/{nonExistentId}");

        // Assert
        await Verify(response, _verifySettings);
    }    [Fact]
    public async Task GetById_WhenInvalidGuid_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/invalid-guid");

        // Assert
        await Verify(response, _verifySettings);
    }    
    
    [Fact]
    public async Task GetById_WhenQsoHasParticipants_ShouldReturnQsoWithParticipants()
    {
        // Arrange - Créer un QSO et ajouter des participants
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO avec Participants",
            Description = "QSO pour tester avec participants",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Ajouter des participants
        var participant1 = new { CallSign = "F1ABC" };
        var participant2 = new { CallSign = "F2DEF" };
          await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", participant1);
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", participant2);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(response, _verifySettings);
    }    
    
    [Fact]
    public async Task GetAll_Performance_ShouldHandleMultipleQsos()
    {
        // Arrange - Créer plusieurs QSO pour tester les performances
        var qsos = new List<object>();
        for (int i = 1; i <= 10; i++)
        {
            qsos.Add(new
            {
                Id = Guid.NewGuid(),
                Name = $"QSO Performance Test {i}",
                Description = $"QSO {i} pour test de performance",
                ModeratorId = Guid.NewGuid()
            });
        }        // Créer tous les QSO
        foreach (var qso in qsos)
        {
            await _client.PostAsJsonAsync("/api/QsoAggregate", qso);
        }

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate");

        // Assert
        await Verify(response, _verifySettings);
    }    
    
    [Fact]
    public async Task GetById_AfterModifications_ShouldReturnUpdatedQso()
    {
        // Arrange - Créer un QSO et le modifier
        var qsoId = Guid.NewGuid();
        var createRequest = new
        {
            Id = qsoId,
            Name = "QSO à Modifier",
            Description = "QSO qui sera modifié",
            ModeratorId = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Ajouter un participant
        var participant = new { CallSign = "F1ABC" };
        await _client.PostAsJsonAsync($"/api/QsoAggregate/{qsoId}/participants", participant);        // Supprimer le participant
        await _client.DeleteAsync($"/api/QsoAggregate/{qsoId}/participants/F1ABC");
        
        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act - Récupérer le QSO après modifications
        var response = await _client.GetAsync($"/api/QsoAggregate/{qsoId}");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WhenNameParameterMissing_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WhenNameParameterEmpty_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search?name=");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WhenNoMatches_ShouldReturnEmptyList()
    {
        // Arrange - Créer quelques QSO qui ne correspondent pas
        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Alpha",
            Description = "Premier QSO",
            ModeratorId = Guid.NewGuid()
        };       
        
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search?name=Beta");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WhenMatchesFound_ShouldReturnMatchingQsos()
    {
        // Arrange - Créer plusieurs QSO avec des noms différents
        var qso1 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test Alpha",
            Description = "Premier QSO pour test de recherche",
            ModeratorId = Guid.NewGuid()
        };

        var qso2 = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Test Beta", 
            Description = "Deuxième QSO pour test de recherche",
            ModeratorId = Guid.NewGuid()
        };

        var qso3 = new
        {
            Id = Guid.NewGuid(),
            Name = "Autre QSO Différent",
            Description = "QSO qui ne devrait pas être trouvé",
            ModeratorId = Guid.NewGuid()
        };        await _client.PostAsJsonAsync("/api/QsoAggregate", qso1);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso2);
        await _client.PostAsJsonAsync("/api/QsoAggregate", qso3);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search?name=Test");

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByName_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange - Créer un QSO avec des caractères spéciaux
        var qsoId = Guid.NewGuid();
        var qso = new
        {
            Id = qsoId,
            Name = "QSO-Test_Spécial",
            Description = "QSO avec caractères spéciaux",
            ModeratorId = Guid.NewGuid()
        };        await _client.PostAsJsonAsync("/api/QsoAggregate", qso);

        // Attendre que les projections soient mises à jour
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search?name=Spécial");

        // Assert
        await Verify(response, _verifySettings);
    }
}
