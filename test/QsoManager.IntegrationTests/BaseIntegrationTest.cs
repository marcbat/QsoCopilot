using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QsoManager.Api;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Testcontainers.MongoDb;
using DotNet.Testcontainers.Builders;

namespace QsoManager.IntegrationTests;

public class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<MongoDbTestFixture>, IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly MongoDbTestFixture _mongoFixture;
    protected MongoDbContainer _containerMongo = null!;
    protected HttpClient _client = null!;
    
    protected readonly VerifySettings _verifySettings;    
    public BaseIntegrationTest(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture)
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

        _factory = factory;
        _mongoFixture = mongoFixture;
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.InitializePlugins();
    }    
    public async Task InitializeAsync()
    {
        // Création du container MongoDB avec le port partagé du fixture
        _containerMongo = new MongoDbBuilder()
            .WithImage("mongo:7.0.4")
            .WithPortBinding(_mongoFixture.Port, 27017)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "password")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(27017)
                .UntilCommandIsCompleted("mongosh", "--eval", "db.adminCommand('ping').ok", "--authenticationDatabase", "admin", "-u", "admin", "-p", "password"))
            .Build();

        await _containerMongo.StartAsync();

        var host = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:MongoDB", _mongoFixture.ConnectionString);
            builder.UseSetting("Mongo:Database", "QsoManagerIntegrationTests");
        });

        _client = host.CreateClient();
    }    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_containerMongo != null)
        {
            await _containerMongo.StopAsync();
            await _containerMongo.DisposeAsync();
        }
    }

    /// <summary>
    /// Crée un utilisateur et un modérateur, puis authentifie le client HTTP avec le token JWT
    /// </summary>
    /// <param name="callSign">L'indicatif à utiliser comme nom d'utilisateur et CallSign</param>
    /// <returns>L'ID de l'utilisateur/modérateur créé et le token JWT</returns>
    protected async Task<(Guid userId, string token)> CreateAndAuthenticateUserAsync(string callSign = "F4TEST")
    {
        // Créer l'utilisateur (qui créera automatiquement un modérateur)
        var registerRequest = new
        {
            UserName = callSign,
            Password = "Test123!@#",
            Email = $"{callSign.ToLower()}@example.com"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        using var registerDocument = JsonDocument.Parse(registerContent);
        var userIdString = registerDocument.RootElement.GetProperty("userId").GetString();
        var userId = Guid.Parse(userIdString!);

        // Se connecter pour obtenir le token JWT
        var loginRequest = new
        {
            UserName = callSign,
            Password = "Test123!@#"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        using var loginDocument = JsonDocument.Parse(loginContent);
        var token = loginDocument.RootElement.GetProperty("token").GetString()!;

        // Configurer le client HTTP avec le token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (userId, token);
    }

    /// <summary>
    /// Supprime l'en-tête d'autorisation du client HTTP
    /// </summary>
    protected void ClearAuthentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Méthode legacy pour maintenir la compatibilité avec les anciens tests
    /// </summary>
    protected async Task<Guid> CreateValidModeratorAsync(string callSign = "F4TEST")
    {
        var (userId, _) = await CreateAndAuthenticateUserAsync(callSign);
        ClearAuthentication(); // Nettoyer l'authentification pour les tests qui n'en ont pas besoin
        return userId;
    }
}
