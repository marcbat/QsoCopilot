using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.Auth;

[Collection("Integration Tests")]
public class AuthControllerResetPasswordTests : BaseIntegrationTest
{
    public AuthControllerResetPasswordTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenInvalidToken()
    {
        // Arrange
        var resetPasswordRequest = new ResetPasswordRequestDto
        {
            UserId = "invalid-user-id",
            ResetToken = "invalid-token",
            Password = "NewPassword123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetPasswordRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenUserIdDoesNotExist()
    {
        // Arrange
        var resetPasswordRequest = new ResetPasswordRequestDto
        {
            UserId = "675e9f1a2b4c8d7e6f8a9b0c", // Valid ObjectId format but non-existent
            ResetToken = "some-token",
            Password = "NewPassword123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetPasswordRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenPasswordTooWeak()
    {
        // Arrange
        var resetPasswordRequest = new ResetPasswordRequestDto
        {
            UserId = "675e9f1a2b4c8d7e6f8a9b0c",
            ResetToken = "some-token",
            Password = "weak" // Weak password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetPasswordRequest);

        // Assert
        await Verify(response, _verifySettings);
    }
}
