using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net.Http.Json;

namespace QsoManager.IntegrationTests;

public class DatabaseCleanupTests : BaseIntegrationTest
{
    public DatabaseCleanupTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task Test1_DatabaseShouldBeEmptyInitially()
    {
        // Arrange & Act
        var isEmpty = await IsDatabaseEmptyAsync();

        // Assert
        Assert.True(isEmpty, "La base de données devrait être vide au début du test");
    }

    [Fact]
    public async Task Test2_DatabaseShouldBeEmptyAfterPreviousTest()
    {
        // Ce test vérifie que la base de données a été nettoyée après le test précédent
        
        // Arrange & Act
        var isEmpty = await IsDatabaseEmptyAsync();

        // Assert
        Assert.True(isEmpty, "La base de données devrait être vide après le nettoyage du test précédent");
    }

    [Fact]
    public async Task Test3_CreateDataAndVerifyCleanupForNextTest()
    {
        // Arrange - Créer des données
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4CLEANUP");
        var createRequest = new
        {
            Id = Guid.NewGuid(),
            Name = "QSO Cleanup Test",
            Description = "Ce QSO devrait être supprimé avant le test suivant"
        };

        // Act - Créer un QSO
        var response = await _client.PostAsJsonAsync("/api/QsoAggregate", createRequest);

        // Assert - Vérifier que le QSO a été créé
        response.EnsureSuccessStatusCode();
        var isEmpty = await IsDatabaseEmptyAsync();
        Assert.False(isEmpty, "La base de données ne devrait pas être vide après création de données");
    }

    [Fact]
    public async Task Test4_VerifyCleanupWorked()
    {
        // Ce test vérifie que les données du test précédent ont bien été nettoyées
        
        // Arrange & Act
        var isEmpty = await IsDatabaseEmptyAsync();

        // Assert
        Assert.True(isEmpty, "La base de données devrait être vide - le nettoyage a échoué");
    }
}
