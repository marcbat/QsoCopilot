using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Api;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace QsoManager.IntegrationTests.Controllers.Auth;

public class AuthControllerUpdateProfileTests : BaseIntegrationTest
{
    public AuthControllerUpdateProfileTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) 
        : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task UpdateProfile_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            Email = "updated@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WhenAuthenticatedWithValidData_ShouldUpdateProfile()
    {
        // Arrange
        var callSign = "F4UPDATE";
        var (userId, token) = await CreateAndAuthenticateUserAsync(callSign);

        var request = new
        {
            Email = "newemail@example.com",
            QrzUsername = "testuser",
            QrzPassword = "testpassword"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var moderatorDto = JsonSerializer.Deserialize<JsonElement>(content);
        
        moderatorDto.GetProperty("id").GetString().Should().Be(userId.ToString());
        moderatorDto.GetProperty("callSign").GetString().Should().Be(callSign);
        moderatorDto.GetProperty("email").GetString().Should().Be("newemail@example.com");
    }

    [Fact]
    public async Task UpdateProfile_WhenUpdatingOnlyEmail_ShouldUpdateEmail()
    {
        // Arrange
        var callSign = "F4EMAIL";
        var (userId, token) = await CreateAndAuthenticateUserAsync(callSign);

        var request = new
        {
            Email = "emailonly@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var moderatorDto = JsonSerializer.Deserialize<JsonElement>(content);
        
        moderatorDto.GetProperty("email").GetString().Should().Be("emailonly@example.com");
    }

    [Fact]
    public async Task UpdateProfile_WhenUpdatingOnlyQrzCredentials_ShouldUpdateQrzCredentials()
    {
        // Arrange
        var callSign = "F4QRZ";
        var (userId, token) = await CreateAndAuthenticateUserAsync(callSign);

        var request = new
        {
            QrzUsername = "qrzuser",
            QrzPassword = "qrzpass"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var moderatorDto = JsonSerializer.Deserialize<JsonElement>(content);
        
        moderatorDto.GetProperty("id").GetString().Should().Be(userId.ToString());
        moderatorDto.GetProperty("callSign").GetString().Should().Be(callSign);
    }
}
