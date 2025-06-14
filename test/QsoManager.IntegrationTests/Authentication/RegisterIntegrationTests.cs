using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;

namespace QsoManager.IntegrationTests.Authentication;

public class RegisterIntegrationTests : BaseIntegrationTest
{
    public RegisterIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldCreateUserAndModerator_WhenValidData()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1ABC",
            Password = "Test123!@#",
            Email = "f1abc@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldFailWithBadRequest_WhenUserAlreadyExists()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1DEF",
            Password = "Test123!@#",
            Email = "f1def@example.com"
        };

        // Premier enregistrement
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Tentative de second enregistrement avec le même username
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(secondResponse, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldFailWithBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var firstRegisterRequest = new RegisterRequestDto
        {
            UserName = "F1GHI",
            Password = "Test123!@#",
            Email = "shared@example.com"
        };

        var secondRegisterRequest = new RegisterRequestDto
        {
            UserName = "F1JKL",
            Password = "Test123!@#",
            Email = "shared@example.com" // Même email
        };

        // Premier enregistrement
        await _client.PostAsJsonAsync("/api/auth/register", firstRegisterRequest);

        // Act - Tentative de second enregistrement avec le même email
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", secondRegisterRequest);

        // Assert
        await Verify(secondResponse, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldFailWithBadRequest_WhenPasswordTooWeak()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1MNO",
            Password = "weak", // Mot de passe trop faible
            Email = "f1mno@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldCreateModeratorWithCallSign_WhenUsernameIsLowercase()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "f1pqr", // Minuscules
            Password = "Test123!@#",
            Email = "f1pqr@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Register_And_Login_ShouldWorkWithCreatedUser()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1STU",
            Password = "Test123!@#",
            Email = "f1stu@example.com"
        };

        // Act - Enregistrement
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Vérification immédiate avec login
        var loginRequest = new LoginRequestDto
        {
            UserName = registerRequest.UserName,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert - Vérifier les deux réponses
        var result = new
        {
            RegisterResponse = registerResponse,
            LoginResponse = loginResponse
        };

        await Verify(result, _verifySettings);
    }
}
