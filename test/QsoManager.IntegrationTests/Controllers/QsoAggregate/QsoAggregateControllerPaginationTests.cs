using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using QsoManager.Application.Common;
using QsoManager.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace QsoManager.IntegrationTests.Controllers.QsoAggregate;

[Collection("Integration Tests")]
public class QsoAggregateControllerPaginationTests : BaseIntegrationTest
{
    public QsoAggregateControllerPaginationTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) 
        : base(factory, mongoFixture)
    {
    }    [Fact]
    public async Task GetAllPaginated_WithDefaultParameters_ReturnsPagedResult()
    {
        // Arrange
        await SeedTestQsos();
        
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAllPaginated_WithCustomPageSize_ReturnsCorrectPageSize()
    {
        // Arrange
        await SeedTestQsos();
        
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated?pageSize=5");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAllPaginated_WithPageNumber_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestQsos();
        
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated?pageNumber=2&pageSize=3");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAllPaginated_WithSortBy_ReturnsSortedResults()
    {
        // Arrange
        await SeedTestQsos();
        
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated?sortBy=Name&sortOrder=desc");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact] 
    public async Task GetAllPaginated_WithInvalidPageNumber_ReturnsEmptyResult()
    {
        // Arrange
        await SeedTestQsos();
        
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated?pageNumber=999");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAllPaginated_WithNegativePageNumber_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated?pageNumber=-1");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task GetAllPaginated_WithZeroPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/paginated?pageSize=0");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchByNamePaginated_WithValidName_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestQsos();
        
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/search/paginated?name=Test");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task SearchMyModeratedPaginated_RequiresAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/api/QsoAggregate/my-moderated/paginated");
        
        // Assert
        await Verify(response, _verifySettings);
    }

    private async Task SeedTestQsos()
    {
        // Create authenticated user and add some QSO aggregates
        var (userId, token) = await CreateAndAuthenticateUserAsync("F4TEST");
        
        var qsos = new[]
        {
            new
            {
                Id = Guid.NewGuid(),
                Name = "QSO Test Alpha",
                Description = "First test QSO for pagination",
                Frequency = 14.205m
            },
            new
            {
                Id = Guid.NewGuid(),
                Name = "QSO Test Beta", 
                Description = "Second test QSO for pagination",
                Frequency = 7.040m
            },
            new
            {
                Id = Guid.NewGuid(),
                Name = "QSO Test Gamma",
                Description = "Third test QSO for pagination", 
                Frequency = 3.580m
            },
            new
            {
                Id = Guid.NewGuid(),
                Name = "Another QSO",
                Description = "Fourth test QSO with different name pattern",
                Frequency = 21.205m
            },
            new
            {
                Id = Guid.NewGuid(),
                Name = "QSO Test Delta",
                Description = "Fifth test QSO for pagination",
                Frequency = 28.405m
            }
        };        foreach (var qso in qsos)
        {
            await _client.PostAsJsonAsync("/api/QsoAggregate", qso);        }
    }
}
