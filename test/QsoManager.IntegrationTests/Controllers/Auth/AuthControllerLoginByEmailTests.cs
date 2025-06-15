using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;

namespace QsoManager.IntegrationTests.Controllers.Auth;

public class AuthControllerLoginByEmailTests : BaseIntegrationTest
{
    public AuthControllerLoginByEmailTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task LoginByEmail_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1EMAIL",
            Password = "Test123!@#",
            Email = "f1email@example.com"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginByEmailRequestDto
        {
            Email = "f1email@example.com",
            Password = "Test123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login-email", loginRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task LoginByEmail_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
        var loginRequest = new LoginByEmailRequestDto
        {
            Email = "invalid@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login-email", loginRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task LoginByEmail_ShouldReturnUnauthorized_WhenEmailDoesNotExist()
    {
        // Arrange
        var loginRequest = new LoginByEmailRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "Test123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login-email", loginRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
