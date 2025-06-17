using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QsoManager.Application.Commands.QsoAggregate;
using QsoManager.Application.DTOs;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using QsoManager.Domain.Entities;
using QsoManager.Domain.Repositories;
using System.Security.Claims;
using System.Threading.Channels;
using Xunit;

namespace QsoManager.Application.UnitTests.Commands.QsoAggregate;

public class AddParticipantCommandHandlerTests
{
    private readonly IQsoAggregateRepository _mockRepository;
    private readonly Channel<IEvent> _mockChannel;
    private readonly ILogger<AddParticipantCommandHandler> _mockLogger;
    private readonly AddParticipantCommandHandler _handler;

    public AddParticipantCommandHandlerTests()
    {
        _mockRepository = Substitute.For<IQsoAggregateRepository>();
        _mockChannel = Channel.CreateUnbounded<IEvent>();
        _mockLogger = Substitute.For<ILogger<AddParticipantCommandHandler>>();
        _handler = new AddParticipantCommandHandler(_mockRepository, _mockChannel, _mockLogger);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddParticipantSuccessfully()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);
        _mockRepository.SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>()).Returns(LanguageExt.Unit.Default);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Match(success => success, errors => throw new Exception("Should not fail"));
        Assert.Equal(aggregateId, dto.Id);
        Assert.Equal(callSign, dto.Participants.First().CallSign);
        Assert.Equal(1, dto.Participants.First().Order);

        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.Received(1).SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenUserNotModerator_ShouldReturnAuthorizationError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid(); // Different from userId
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, moderatorId);
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("Vous n'êtes pas autorisé à modifier ce QSO", errors.First().Message);

        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenUserIdClaimMissing_ShouldReturnAuthenticationError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = new ClaimsPrincipal(); // No claims
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("Utilisateur non authentifié", errors.First().Message);

        await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenUserIdClaimInvalid_ShouldReturnAuthenticationError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "invalid-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("ID utilisateur invalide", errors.First().Message);

        await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenAggregateNotFound_ShouldReturnRepositoryError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        _mockRepository.GetByIdAsync(aggregateId).Returns(Error.New("Aggregate not found"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("Aggregate not found", errors.First().Message);

        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenAddParticipantFails_ShouldReturnDomainError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = ""; // Invalid call sign
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("L'indicatif ne peut pas être vide", errors.First().Message);

        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenSaveAggregateFails_ShouldReturnRepositoryError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);
        _mockRepository.SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>()).Returns(Error.New("Save failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("Save failed", errors.First().Message);

        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.Received(1).SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldReturnGenericError()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        _mockRepository.GetByIdAsync(aggregateId).Returns(Task.FromException<Validation<Error, Domain.Aggregates.QsoAggregate>>(new Exception("Unexpected error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        Assert.Contains("Impossible d'ajouter le participant", errors.First().Message);        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    private static Domain.Aggregates.QsoAggregate CreateQsoAggregate(Guid aggregateId, Guid moderatorId)
    {
        var aggregate = Domain.Aggregates.QsoAggregate.Create(
            aggregateId, 
            "Test QSO", 
            "Test Description", 
            moderatorId);

        return aggregate.Match(
            success => success,
            errors => throw new Exception($"Failed to create aggregate: {string.Join(", ", errors.Select(e => e.Message))}")
        );
    }
}
