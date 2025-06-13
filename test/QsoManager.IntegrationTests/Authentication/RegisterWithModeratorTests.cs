using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QsoManager.Application.Commands.ModeratorAggregate;
using QsoManager.Application.DTOs.Authentication;
using QsoManager.Domain.Repositories;
using System.Net.Http.Json;
using System.Text.Json;

namespace QsoManager.IntegrationTests.Authentication;

public class RegisterWithModeratorTests : BaseIntegrationTest
{
    public RegisterWithModeratorTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldCreateUserAndAssociatedModerator()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F1ABC",
            Password = "Test123!",
            Email = "f1abc@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(content);
        
        // Vérifier qu'un modérateur a été créé avec le bon CallSign
        using var scope = _factory.Services.CreateScope();
        var moderatorRepository = scope.ServiceProvider.GetRequiredService<IModeratorAggregateRepository>();
        
        var moderatorResult = await moderatorRepository.GetByCallSignAsync("F1ABC");
        
        // Vérifier que le modérateur existe
        moderatorResult.IsSuccess.Should().BeTrue();
        var moderator = moderatorResult.IfFail(() => null!);
        moderator.Should().NotBeNull();
        moderator!.CallSign.Should().Be("F1ABC");
        moderator.Email.Should().Be("f1abc@example.com");
    }

    [Fact]
    public async Task Register_ShouldFailIfCallSignAlreadyExists()
    {
        // Arrange
        var registerRequest1 = new RegisterRequestDto
        {
            UserName = "F2XYZ",
            Password = "Test123!",
            Email = "f2xyz@example.com"
        };

        var registerRequest2 = new RegisterRequestDto
        {
            UserName = "F2XYZ",  // Même CallSign
            Password = "Test456!",
            Email = "f2xyz_different@example.com"
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", registerRequest1);
        var response2 = await _client.PostAsJsonAsync("/api/auth/register", registerRequest2);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.IsSuccessStatusCode.Should().BeFalse(); // Le deuxième devrait échouer
    }

    [Fact]
    public async Task Register_WithoutEmail_ShouldCreateModeratorWithoutEmail()
    {
        // Arrange
        var registerRequest = new RegisterRequestDto
        {
            UserName = "F3DEF",
            Password = "Test123!",
            Email = "" // Email vide
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Vérifier le modérateur créé
        using var scope = _factory.Services.CreateScope();
        var moderatorRepository = scope.ServiceProvider.GetRequiredService<IModeratorAggregateRepository>();
        
        var moderatorResult = await moderatorRepository.GetByCallSignAsync("F3DEF");
        
        moderatorResult.IsSuccess.Should().BeTrue();
        var moderator = moderatorResult.IfFail(() => null!);
        moderator.Should().NotBeNull();
        moderator!.CallSign.Should().Be("F3DEF");
        moderator.Email.Should().BeNull();
    }
}
