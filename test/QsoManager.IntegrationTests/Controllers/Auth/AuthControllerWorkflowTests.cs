using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;

namespace QsoManager.IntegrationTests.Controllers.Auth;

public class AuthControllerWorkflowTests : BaseIntegrationTest
{
    public AuthControllerWorkflowTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task AuthWorkflow_Register_Login_ShouldWork()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1WORKFLOW",
            Password = "Test123!@#",
            Email = "f1workflow@example.com"
        };

        // Act - Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Login
        var loginRequest = new LoginRequestDto
        {
            UserName = registerRequest.UserName,
            Password = registerRequest.Password
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Act - Login by Email
        var loginByEmailRequest = new LoginByEmailRequestDto
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginByEmailResponse = await _client.PostAsJsonAsync("/api/auth/login-email", loginByEmailRequest);

        // Assert
        var result = new
        {
            RegisterResponse = registerResponse,
            LoginResponse = loginResponse,
            LoginByEmailResponse = loginByEmailResponse
        };

        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AuthWorkflow_Register_ForgotPassword_ShouldWork()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1FORGOT2",
            Password = "Test123!@#",
            Email = "f1forgot2@example.com"
        };

        // Act - Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Forgot Password
        var forgotPasswordRequest = new ForgotPasswordRequestDto
        {
            Email = registerRequest.Email,
            ResetPasswordUrl = "https://example.com/reset-password"
        };
        var forgotPasswordResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

        // Assert
        var result = new
        {
            RegisterResponse = registerResponse,
            ForgotPasswordResponse = forgotPasswordResponse
        };

        await Verify(result, _verifySettings);
    }
}
