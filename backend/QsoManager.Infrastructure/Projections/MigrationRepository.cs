using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QsoManager.Application.Projections.Interfaces;
using static LanguageExt.Prelude;

namespace QsoManager.Infrastructure.Projections;

public class MigrationRepository : IMigrationRepository
{
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<MigrationRepository> _logger;
    private readonly string _databaseName;

    public MigrationRepository(
        IMongoClient mongoClient,
        IConfiguration configuration,
        ILogger<MigrationRepository> logger)
    {
        _mongoClient = mongoClient;
        _logger = logger;
        _databaseName = configuration["Mongo:Database"] ?? "QsoManager";
    }

    public async Task<Validation<Error, Unit>> ResetProjectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting projections database reset for database {Database}", _databaseName);

            var database = _mongoClient.GetDatabase(_databaseName);
            
            // Liste des collections de projections à supprimer
            var projectionCollections = new[]
            {
                "QsoAggregateProjections"
                // Ajouter d'autres collections de projections ici si nécessaire
            };

            foreach (var collectionName in projectionCollections)
            {
                try
                {
                    await database.DropCollectionAsync(collectionName, cancellationToken);
                    _logger.LogInformation("Dropped projection collection {Collection}", collectionName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to drop collection {Collection} (might not exist)", collectionName);
                }
            }

            _logger.LogInformation("Projections database reset completed successfully");
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting projections database {Database}", _databaseName);
            return Error.New($"Failed to reset projections database: {ex.Message}");
        }
    }
}
