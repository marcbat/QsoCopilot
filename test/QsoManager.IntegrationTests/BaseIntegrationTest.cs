using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QsoManager.Api;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit;

namespace QsoManager.IntegrationTests;

[Collection("Integration Tests")]
public class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly MongoDbTestFixture _mongoFixture;
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
        // Nettoyer la base de données avant chaque test
        await _mongoFixture.CleanDatabaseAsync();        var host = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:MongoDB", _mongoFixture.ConnectionString);
            builder.UseSetting("Mongo:Database", "QsoManagerIntegrationTests");
            
            // Configuration QRZ pour les tests - lire depuis les variables d'environnement ou configuration
            var qrzUsername = Environment.GetEnvironmentVariable("QRZ_TEST_USERNAME");
            var qrzPassword = Environment.GetEnvironmentVariable("QRZ_TEST_PASSWORD");
            
            if (!string.IsNullOrEmpty(qrzUsername))
            {
                builder.UseSetting("QRZ:TestCredentials:Username", qrzUsername);
            }
            if (!string.IsNullOrEmpty(qrzPassword))
            {
                builder.UseSetting("QRZ:TestCredentials:Password", qrzPassword);
            }
        });

        _client = host.CreateClient();
    }    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
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
    }    /// <summary>
    /// Méthode helper pour vérifier que la base de données est vide (utile pour tester le nettoyage)
    /// </summary>
    protected async Task<bool> IsDatabaseEmptyAsync()
    {
        var database = _mongoFixture.Database;
        var collectionNames = await database.ListCollectionNamesAsync();
        var collections = new List<string>();
        
        while (await collectionNames.MoveNextAsync())
        {
            collections.AddRange(collectionNames.Current);
        }
        
        if (!collections.Any()) return true;
        
        // Vérifier que toutes les collections sont vides
        foreach (var collectionName in collections)
        {
            var collection = database.GetCollection<object>(collectionName);
            var count = await collection.CountDocumentsAsync("{}");
            if (count > 0) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Crée un utilisateur avec des credentials QRZ pour les tests
    /// </summary>
    /// <param name="callSign">L'indicatif à utiliser</param>
    /// <param name="qrzUsername">Nom d'utilisateur QRZ</param>
    /// <param name="qrzPassword">Mot de passe QRZ (sera stocké en clair pour les tests)</param>
    /// <returns>L'ID de l'utilisateur/modérateur créé et le token JWT</returns>
    protected async Task<(Guid userId, string token)> CreateAndAuthenticateUserWithQrzAsync(
        string callSign = "F4TEST", 
        string? qrzUsername = null, 
        string? qrzPassword = null)
    {
        var (userId, token) = await CreateAndAuthenticateUserAsync(callSign);

        // Mettre à jour les credentials QRZ si fournis
        if (!string.IsNullOrEmpty(qrzUsername) || !string.IsNullOrEmpty(qrzPassword))
        {
            var updateRequest = new
            {
                QrzUsername = qrzUsername,
                QrzPassword = qrzPassword
            };

            var updateResponse = await _client.PutAsJsonAsync("/api/auth/profile", updateRequest);
            updateResponse.EnsureSuccessStatusCode();
        }

        return (userId, token);
    }
}
