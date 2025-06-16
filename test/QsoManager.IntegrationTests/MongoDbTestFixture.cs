using Testcontainers.MongoDb;
using DotNet.Testcontainers.Builders;
using MongoDB.Driver;

namespace QsoManager.IntegrationTests;

public class MongoDbTestFixture : IAsyncLifetime
{
    public int Port { get; private set; }
    public string ConnectionString => $"mongodb://admin:password@localhost:{Port}";
    
    private MongoDbContainer? _mongoContainer;
    private IMongoClient? _mongoClient;
    private IMongoDatabase? _database;

    public IMongoDatabase Database => _database ?? throw new InvalidOperationException("Database not initialized");

    public MongoDbTestFixture()
    {
        // Génération d'un port aléatoire une seule fois pour toute la classe de tests
        var random = new Random();
        Port = random.Next(10000, 30001);
    }

    public async Task InitializeAsync()
    {
        // Création du container MongoDB une seule fois pour toute la classe de tests
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0.4")
            .WithPortBinding(Port, 27017)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "password")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(27017)
                .UntilCommandIsCompleted("mongosh", "--eval", "db.adminCommand('ping').ok", "--authenticationDatabase", "admin", "-u", "admin", "-p", "password"))
            .Build();

        await _mongoContainer.StartAsync();

        // Initialiser le client MongoDB et la base de données
        _mongoClient = new MongoClient(ConnectionString);
        _database = _mongoClient.GetDatabase("QsoManagerIntegrationTests");
    }   
    
    public async Task DisposeAsync()
    {
        if (_mongoContainer != null)
        {
            await _mongoContainer.StopAsync();
            await _mongoContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Nettoie toutes les collections de la base de données
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (_database == null) return;

        var collectionNames = await _database.ListCollectionNamesAsync();
        await collectionNames.ForEachAsync(async collectionName =>
        {
            await _database.DropCollectionAsync(collectionName);
        });
    }
}
