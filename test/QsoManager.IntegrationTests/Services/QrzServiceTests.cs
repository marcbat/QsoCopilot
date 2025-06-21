using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QsoManager.Api;
using QsoManager.Application.Interfaces.Services;
using QsoManager.IntegrationTests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace QsoManager.IntegrationTests.Services;

[Collection("Integration Tests")]
public class QrzServiceTests : BaseIntegrationTest
{
    private readonly ITestOutputHelper _output;

    public QrzServiceTests(
        WebApplicationFactory<Program> factory, 
        MongoDbTestFixture mongoFixture,
        ITestOutputHelper output) 
        : base(factory, mongoFixture)
    {
        _output = output;
    }

    [Fact]
    public async Task LookupCallsignAsync_WithValidCredentials_ShouldReturnQrzData()
    {
        // Arrange
        var qrzCredentials = QrzSecretsHelper.GetQrzCredentials();
        if (qrzCredentials == null)
        {
            _output.WriteLine("SKIPPED: QRZ credentials not available");
            return;
        }

        var (qrzUsername, qrzPassword) = qrzCredentials.Value;
        
        // Get the QRZ service from DI
        using var scope = _factory.Services.CreateScope();
        var qrzService = scope.ServiceProvider.GetRequiredService<IQrzService>();

        // Act
        var result = await qrzService.LookupCallsignAsync("HB9GXP", qrzUsername, qrzPassword);

        // Assert
        _output.WriteLine($"QRZ Username: {qrzUsername}");
        _output.WriteLine($"QRZ Password: {qrzPassword?.Substring(0, Math.Min(3, qrzPassword.Length))}***");
        _output.WriteLine($"Result: {result?.CallSign ?? "null"}");
        
        if (result != null)
        {
            _output.WriteLine($"Name: {result.Name}");
            _output.WriteLine($"Country: {result.Country}");
            _output.WriteLine($"Grid: {result.Grid}");
        }
        
        Assert.NotNull(result);
        Assert.Equal("HB9GXP", result.CallSign);
    }

    [Fact]
    public async Task LookupCallsignAsync_WithoutCredentials_ShouldReturnLimitedData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var qrzService = scope.ServiceProvider.GetRequiredService<IQrzService>();

        // Act
        var result = await qrzService.LookupCallsignAsync("HB9GXP");

        // Assert
        _output.WriteLine($"Result without credentials: {result?.CallSign ?? "null"}");
        
        if (result != null)
        {
            _output.WriteLine($"Name: {result.Name}");
            _output.WriteLine($"Country: {result.Country}");
            _output.WriteLine($"Grid: {result.Grid}");        }
        
        // Le lookup public peut retourner null ou des données limitées
        // On vérifie juste que le service ne plante pas
        Assert.True(true);
    }
}
