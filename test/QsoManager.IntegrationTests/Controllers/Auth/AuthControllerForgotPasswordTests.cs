using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;
using Xunit;

namespace QsoManager.IntegrationTests.Controllers.Auth;

[Collection("Integration Tests")]
public class AuthControllerForgotPasswordTests : BaseIntegrationTest
{
    public AuthControllerForgotPasswordTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnSuccess_WhenEmailExists()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1FORGOT",
            Password = "Test123!@#",
            Email = "f1forgot@example.com"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var forgotPasswordRequest = new ForgotPasswordRequestDto
        {
            Email = "f1forgot@example.com",
            ResetPasswordUrl = "https://example.com/reset"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailDoesNotExist()
    {
        // Arrange
        var forgotPasswordRequest = new ForgotPasswordRequestDto
        {
            Email = "nonexistent@example.com",
            ResetPasswordUrl = "https://example.com/reset"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenInvalidResetUrl()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1RESET",
            Password = "Test123!@#",
            Email = "f1reset@example.com"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var forgotPasswordRequest = new ForgotPasswordRequestDto
        {
            Email = "f1reset@example.com",
            ResetPasswordUrl = "invalid-url"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
