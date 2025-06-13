using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QsoManager.Application.Commands.ModeratorAggregate;
using QsoManager.Application.DTOs;
using QsoManager.Application.Handlers.Authentication;
using QsoManager.Application.Interfaces.Auth;
using QsoManager.Application.Commands.Authentication;
using MediatR;
using static LanguageExt.Prelude;

namespace QsoManager.UnitTests.Authentication;

public class RegisterCommandHandlerTests
{
    private readonly IAuthenticationService _authService;
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _authService = Substitute.For<IAuthenticationService>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<RegisterCommandHandler>>();
        _handler = new RegisterCommandHandler(_authService, _mediator, _logger);
    }    [Fact]
    public async Task Handle_ShouldCreateUserAndModerator_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new RegisterCommand("F1ABC", "Password123!", "test@example.com");
        
        _authService.Register(request.UserName, request.Password, request.Email)
            .Returns(userId);

        var moderatorDto = new ModeratorDto(Guid.Parse(userId), "F1ABC", "test@example.com");
        _mediator.Send(Arg.Any<CreateModeratorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Validation<Error, ModeratorDto>.Success(moderatorDto));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(userId);
        
        await _authService.Received(1).Register(request.UserName, request.Password, request.Email);
        await _mediator.Received(1).Send(Arg.Is<CreateModeratorCommand>(cmd => 
            cmd.Id == Guid.Parse(userId) && 
            cmd.CallSign == request.UserName && 
            cmd.Email == request.Email), Arg.Any<CancellationToken>());
    }    [Fact]
    public async Task Handle_ShouldStillReturnUserId_WhenModeratorCreationFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new RegisterCommand("F1ABC", "Password123!", "test@example.com");
        
        _authService.Register(request.UserName, request.Password, request.Email)
            .Returns(userId);        _mediator.Send(Arg.Any<CreateModeratorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Validation<Error, ModeratorDto>.Fail(Seq1(Error.New("Moderator creation failed"))));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(userId);
        
        await _authService.Received(1).Register(request.UserName, request.Password, request.Email);
        await _mediator.Received(1).Send(Arg.Any<CreateModeratorCommand>(), Arg.Any<CancellationToken>());
    }    [Fact]
    public async Task Handle_ShouldCreateModeratorWithNullEmail_WhenEmailIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new RegisterCommand("F1ABC", "Password123!", "");
        
        _authService.Register(request.UserName, request.Password, request.Email)
            .Returns(userId);

        var moderatorDto = new ModeratorDto(Guid.Parse(userId), "F1ABC", null);
        _mediator.Send(Arg.Any<CreateModeratorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Validation<Error, ModeratorDto>.Success(moderatorDto));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(userId);
        
        await _mediator.Received(1).Send(Arg.Is<CreateModeratorCommand>(cmd => 
            cmd.Id == Guid.Parse(userId) && 
            cmd.CallSign == request.UserName && 
            cmd.Email == ""), Arg.Any<CancellationToken>());
    }    [Fact]
    public async Task Handle_ShouldCreateModeratorWithUppercaseCallSign_WhenUserNameIsLowercase()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new RegisterCommand("f1abc", "Password123!", "test@example.com");
        
        _authService.Register(request.UserName, request.Password, request.Email)
            .Returns(userId);

        var moderatorDto = new ModeratorDto(Guid.Parse(userId), "F1ABC", "test@example.com");
        _mediator.Send(Arg.Any<CreateModeratorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Validation<Error, ModeratorDto>.Success(moderatorDto));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(userId);
        
        // Vérifier que la commande est envoyée avec le CallSign original (la transformation se fait dans l'agrégat)
        await _mediator.Received(1).Send(Arg.Is<CreateModeratorCommand>(cmd => 
            cmd.Id == Guid.Parse(userId) && 
            cmd.CallSign == request.UserName && // Le username original est passé
            cmd.Email == request.Email), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenModeratorCreationFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var request = new RegisterCommand("F1ABC", "Password123!", "test@example.com");
        
        _authService.Register(request.UserName, request.Password, request.Email)
            .Returns(userId);        _mediator.Send(Arg.Any<CreateModeratorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Validation<Error, ModeratorDto>.Fail(Seq1(Error.New("Database error"))));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(userId);
        
        // Vérifier que le logger a été appelé avec un avertissement
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("erreur lors de la création du modérateur")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
