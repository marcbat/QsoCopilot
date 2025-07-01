using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;
using Xunit;


namespace QsoManager.IntegrationTests.Controllers.Auth;

[Collection("Integration Tests")]
public class AuthControllerRegisterTests : BaseIntegrationTest
{
    public AuthControllerRegisterTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    [Fact]
    public async Task Register_ShouldCreateUser_WhenValidData()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1NEW",
            Password = "Test123!@#",
            Email = "f1new@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUserAlreadyExists()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1DUP",
            Password = "Test123!@#",
            Email = "f1dup@example.com"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Second registration with same username
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var firstRequest = new RegisterRequestDto
        {
            UserName = "F1FIRST",
            Password = "Test123!@#",
            Email = "shared@example.com"
        };

        var secondRequest = new RegisterRequestDto
        {
            UserName = "F1SECOND",
            Password = "Test123!@#",
            Email = "shared@example.com" // Same email
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", firstRequest);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", secondRequest);

        // Assert
        await Verify(response, _verifySettings);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordTooWeak()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1WEAK",
            Password = "123", // Weak password
            Email = "f1weak@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        await Verify(response, _verifySettings);
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
    public async Task Register_ShouldFailWithBadRequest_WhenPasswordTooWeakDetailed()
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
