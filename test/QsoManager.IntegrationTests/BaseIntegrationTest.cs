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
        _verifySettings.AutoVerify();

        var random = new Random();
        _randomPort = random.Next(10000, 30001);

        _factory = factory;        _containerMongo = new MongoDbBuilder()
            .WithImage("mongo:7.0.4")
            .WithPortBinding(_randomPort, 27017)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "password")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.InitializePlugins();
    }    public async Task InitializeAsync()
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
