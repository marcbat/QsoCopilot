using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace QsoManager.Infrastructure.Configuration;

/// <summary>
/// Service d'initialisation de la base de données qui gère les différences entre MongoDB et Cosmos DB
/// </summary>
public class DatabaseInitializationService : IHostedService
{
    private readonly IMongoClient _mongoClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly string _databaseName;

    public DatabaseInitializationService(
        IMongoClient mongoClient,
        IConfiguration configuration,
        ILogger<DatabaseInitializationService> logger)
    {
        _mongoClient = mongoClient;
        _configuration = configuration;
        _logger = logger;
        _databaseName = configuration["Mongo:Database"] ?? "QsoManager";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🚀 Starting database initialization for {DatabaseName}", _databaseName);
            
            var database = _mongoClient.GetDatabase(_databaseName);
            
            // Tenter de se connecter à la base de données pour valider la connexion
            await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1), cancellationToken: cancellationToken);
            _logger.LogInformation("✅ Database connection successful");
            
            // Détecter si nous utilisons Cosmos DB ou MongoDB natif
            var isCosmosDb = await IsCosmosDbAsync(database);
            
            if (isCosmosDb)
            {
                _logger.LogInformation("🌌 Detected Azure Cosmos DB - using Cosmos DB compatible index strategy");
                await InitializeCosmosDbIndexesAsync(database, cancellationToken);
            }
            else
            {
                _logger.LogInformation("🍃 Detected native MongoDB - using full MongoDB index strategy");
                await InitializeMongoDbIndexesAsync(database, cancellationToken);
            }
            
            _logger.LogInformation("✅ Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during database initialization");
            // Ne pas faire échouer le démarrage de l'application
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Détecte si nous utilisons Azure Cosmos DB en analysant la variable d'environnement et la chaîne de connexion
    /// </summary>
    private async Task<bool> IsCosmosDbAsync(IMongoDatabase database)
    {
        try
        {
            // Méthode 1: Vérifier la variable d'environnement COSMOS_DB_ENABLED
            var cosmosDbEnabled = _configuration.GetValue<bool>("COSMOS_DB_ENABLED");
            if (cosmosDbEnabled)
            {
                _logger.LogInformation("Cosmos DB detected via COSMOS_DB_ENABLED environment variable");
                return true;
            }

            // Méthode 2: Vérifier la chaîne de connexion
            var connectionString = _configuration.GetConnectionString("MongoDB") ?? "";
            if (connectionString.Contains("cosmos.azure.com") || 
                connectionString.Contains("documents.azure.com"))
            {
                _logger.LogInformation("Cosmos DB detected via connection string analysis");
                return true;
            }

            // Méthode 3: Tenter une commande spécifique à MongoDB pour détecter Cosmos DB
            var command = new MongoDB.Bson.BsonDocument("buildInfo", 1);
            var result = await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(command);
            
            // Si la version contient "cosmos" ou si certaines propriétés sont absentes
            if (result.Contains("version"))
            {
                var version = result["version"].AsString;
                if (version.ToLower().Contains("cosmos"))
                {
                    _logger.LogInformation("Cosmos DB detected via buildInfo command");
                    return true;
                }
            }
            
            _logger.LogInformation("Native MongoDB detected");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not detect database type, assuming MongoDB native");
            return false;
        }
    }

    /// <summary>
    /// Initialise les index pour Azure Cosmos DB (API MongoDB)
    /// </summary>
    private async Task InitializeCosmosDbIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📋 Creating Cosmos DB compatible indexes...");
            
            // Collection Events - Index simples pour Cosmos DB
            var eventsCollection = database.GetCollection<object>("Events");
            
            // Index simple sur AggregateId (Cosmos DB supporte les index simples)
            var aggregateIdIndex = Builders<object>.IndexKeys.Ascending("AggregateId");
            await CreateIndexSafelyAsync(eventsCollection, aggregateIdIndex, "Events_AggregateId");
            
            // Index simple sur Timestamp
            var timestampIndex = Builders<object>.IndexKeys.Ascending("Timestamp");
            await CreateIndexSafelyAsync(eventsCollection, timestampIndex, "Events_Timestamp");
            
            // Collection QsoAggregateProjections - Index simples
            var projectionsCollection = database.GetCollection<object>("QsoAggregateProjections");
            
            // Index simple sur Id
            var idIndex = Builders<object>.IndexKeys.Ascending("_id");
            await CreateIndexSafelyAsync(projectionsCollection, idIndex, "Projections_Id");
            
            // Index simple sur Name pour la recherche (sans regex complexe)
            var nameIndex = Builders<object>.IndexKeys.Ascending("Name");
            await CreateIndexSafelyAsync(projectionsCollection, nameIndex, "Projections_Name");
            
            _logger.LogInformation("✅ Cosmos DB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating Cosmos DB indexes");
        }
    }

    /// <summary>
    /// Initialise les index pour MongoDB natif (toutes les fonctionnalités)
    /// </summary>
    private async Task InitializeMongoDbIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📋 Creating full MongoDB indexes...");
            
            // Collection Events - Index complets pour MongoDB
            var eventsCollection = database.GetCollection<object>("Events");
            
            // Index composé pour AggregateId et Version
            var aggregateVersionIndex = Builders<object>.IndexKeys
                .Ascending("AggregateId")
                .Ascending("Version");
            await CreateIndexSafelyAsync(eventsCollection, aggregateVersionIndex, "Events_AggregateId_Version");
            
            // Index sur Timestamp
            var timestampIndex = Builders<object>.IndexKeys.Ascending("Timestamp");
            await CreateIndexSafelyAsync(eventsCollection, timestampIndex, "Events_Timestamp");
            
            // Index sur EventType
            var eventTypeIndex = Builders<object>.IndexKeys.Ascending("EventType");
            await CreateIndexSafelyAsync(eventsCollection, eventTypeIndex, "Events_EventType");
            
            // Collection QsoAggregateProjections - Index complets
            var projectionsCollection = database.GetCollection<object>("QsoAggregateProjections");
            
            // Index texte pour la recherche par nom (supporté par MongoDB natif)
            var textIndex = Builders<object>.IndexKeys.Text("Name");
            await CreateIndexSafelyAsync(projectionsCollection, textIndex, "Projections_Name_Text");
            
            // Index sur ModeratorId
            var moderatorIndex = Builders<object>.IndexKeys.Ascending("ModeratorId");
            await CreateIndexSafelyAsync(projectionsCollection, moderatorIndex, "Projections_ModeratorId");
            
            _logger.LogInformation("✅ MongoDB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating MongoDB indexes");
        }
    }

    /// <summary>
    /// Crée un index de manière sécurisée (ne fait pas échouer si l'index existe déjà)
    /// </summary>
    private async Task CreateIndexSafelyAsync<T>(IMongoCollection<T> collection, IndexKeysDefinition<T> indexKeys, string indexName)
    {
        try
        {
            var options = new CreateIndexOptions { Name = indexName };
            var indexModel = new CreateIndexModel<T>(indexKeys, options);
            await collection.Indexes.CreateOneAsync(indexModel);
            _logger.LogDebug("Created index {IndexName} on collection {CollectionName}", indexName, collection.CollectionNamespace.CollectionName);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 85) // Index already exists
        {
            _logger.LogDebug("Index {IndexName} already exists on collection {CollectionName}", indexName, collection.CollectionNamespace.CollectionName);
        }
        catch (MongoCommandException ex) when (ex.Code == 85) // Index already exists (alternative error type)
        {
            _logger.LogDebug("Index {IndexName} already exists on collection {CollectionName}", indexName, collection.CollectionNamespace.CollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create index {IndexName} on collection {CollectionName} - {ErrorMessage}", indexName, collection.CollectionNamespace.CollectionName, ex.Message);
        }
    }
}
