using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.Auth;

[Collection("Integration Tests")]
public class AuthControllerLoginTests : BaseIntegrationTest
{
    public AuthControllerLoginTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1TEST",
            Password = "Test123!@#",
            Email = "f1test@example.com"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            UserName = "F1TEST",
            Password = "Test123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            UserName = "INVALID",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            UserName = "NONEXISTENT",
            Password = "Test123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
