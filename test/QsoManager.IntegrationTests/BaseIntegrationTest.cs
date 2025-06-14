using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QsoManager.Api;
using System.Runtime.CompilerServices;
using Testcontainers.MongoDb;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace QsoManager.IntegrationTests;

public class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly int _randomPort;
    protected readonly WebApplicationFactory<Program> _factory;
    protected MongoDbContainer _containerMongo;
    protected HttpClient _client = null!;
    
    protected readonly VerifySettings _verifySettings;    
    
    public BaseIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory(Path.Combine("snapshots"));
        
        // Attention à bien désactiver AutoVerify pour les tests
        // _verifySettings.AutoVerify();        // Scrubbers pour normaliser les valeurs qui changent à chaque test
        _verifySettings.ScrubMember("traceId");
        
        // Scrubber pour les tokens JWT - ils changent à chaque fois car ils contiennent des timestamps
        _verifySettings.ScrubMember("token");
        
        // Scrubber pour les dates d'expiration des tokens
        _verifySettings.ScrubMember("expiration");
        
        // Scrubber pour remplacer les GUIDs par des valeurs consistantes
        _verifySettings.AddScrubber(text => 
        {
            var result = System.Text.RegularExpressions.Regex.Replace(text.ToString(), 
                @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b", 
                "NORMALIZED_GUID");
            text.Clear();
            text.Append(result);
        });

        var random = new Random();
        _randomPort = random.Next(10000, 30001);

        _factory = factory;        _containerMongo = new MongoDbBuilder()
            .WithImage("mongo:7.0.4")
            .WithPortBinding(_randomPort, 27017)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "password")            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(27017)
                .UntilCommandIsCompleted("mongosh", "--eval", "db.adminCommand('ping').ok", "--authenticationDatabase", "admin", "-u", "admin", "-p", "password"))
            .Build();
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.InitializePlugins();
    }    
    
    public async Task InitializeAsync()
    {
        await _containerMongo.StartAsync();

        var host = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:MongoDB", $"mongodb://admin:password@localhost:{_randomPort}");
            builder.UseSetting("Mongo:Database", "QsoManagerIntegrationTests");
        });

        _client = host.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _containerMongo.StopAsync();
        await _containerMongo.DisposeAsync();
    }
}
