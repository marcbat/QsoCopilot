using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QsoManager.Application.DTOs.Authentication;

namespace QsoManager.IntegrationTests.Controllers;

public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(WebApplicationFactory<Program> factory, MongoDbTestFixture mongoFixture) : base(factory, mongoFixture)
    {
    }

    #region Login Tests (POST /api/auth/login)

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

    #endregion

    #region Login by Email Tests (POST /api/auth/login-email)

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

    #endregion

    #region Register Tests (POST /api/auth/register)

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

    #endregion

    #region Forgot Password Tests (POST /api/auth/forgot-password)

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

    #endregion

    #region Reset Password Tests (POST /api/auth/reset-password)

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

    #endregion

    #region Integration Tests - Combined Workflows

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

    #endregion
}
