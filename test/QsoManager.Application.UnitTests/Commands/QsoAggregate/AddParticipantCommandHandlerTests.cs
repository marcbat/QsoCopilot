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
using FluentAssertions;

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
        
        // Clear any potential existing events in the channel
        while (_mockChannel.Reader.TryRead(out _)) { }
    }[Fact]
    public async Task Handle_WhenValidRequest_ShouldAddParticipantSuccessfully()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        aggregate.ClearChanges(); // Clear the initial Created event
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);
        _mockRepository.SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>()).Returns(LanguageExt.Unit.Default);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Match(success => success, errors => throw new Exception("Should not fail"));
        dto.Id.Should().Be(aggregateId);
        dto.Participants.First().CallSign.Should().Be(callSign);
        dto.Participants.First().Order.Should().Be(1);

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("Vous n'êtes pas autorisé à modifier ce QSO");

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("Utilisateur non authentifié");

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("ID utilisateur invalide");

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("Aggregate not found");

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("L'indicatif ne peut pas être vide");

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("Save failed");

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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        var errors = result.Match(success => throw new Exception("Should fail"), errors => errors);
        errors.First().Message.Should().Contain("Impossible d'ajouter le participant");
        
        await _mockRepository.Received(1).GetByIdAsync(aggregateId);
        await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>());
    }    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDispatchParticipantAddedEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        aggregate.ClearChanges(); // Clear the initial Created event
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);
        _mockRepository.SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>()).Returns(LanguageExt.Unit.Default);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify that a ParticipantAdded event was dispatched to the channel
        var reader = _mockChannel.Reader;
        reader.TryRead(out var dispatchedEvent).Should().BeTrue();
        
        var participantAddedEvent = dispatchedEvent.Should().BeOfType<Domain.Aggregates.QsoAggregate.Events.ParticipantAdded>().Subject;
        participantAddedEvent.AggregateId.Should().Be(aggregateId);
        participantAddedEvent.CallSign.Should().Be(callSign);
        participantAddedEvent.Order.Should().Be(1);
        
        // Verify that no more events are in the channel for this test
        reader.TryRead(out _).Should().BeFalse();
    }    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDispatchEventsInCorrectOrder()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        aggregate.ClearChanges(); // Clear the initial Created event
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);
        _mockRepository.SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>()).Returns(LanguageExt.Unit.Default);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify that events are dispatched in the correct order and with correct details
        var reader = _mockChannel.Reader;
        var dispatchedEvents = new List<IEvent>();
        
        while (reader.TryRead(out var evt))
        {
            dispatchedEvents.Add(evt);
        }

        // Should have exactly one ParticipantAdded event
        dispatchedEvents.Should().HaveCount(1);
        
        var participantAddedEvent = dispatchedEvents.OfType<Domain.Aggregates.QsoAggregate.Events.ParticipantAdded>()
            .FirstOrDefault();
        
        participantAddedEvent.Should().NotBeNull();
        participantAddedEvent.AggregateId.Should().Be(aggregateId);
        participantAddedEvent.CallSign.Should().Be(callSign);
        participantAddedEvent.Order.Should().Be(1);
        participantAddedEvent.DateEvent.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }    
    
    [Fact]
    public async Task Handle_WhenValidRequestWithExistingParticipants_ShouldDispatchEventWithCorrectOrder()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
          // Add an existing participant to test order increment
        var existingParticipantResult = aggregate.AddParticipant("F4EXISTING");
        existingParticipantResult.IsSuccess.Should().BeTrue(); // Verify the existing participant was added successfully
        
        aggregate.ClearChanges(); // Clear events from the setup
        
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);
        _mockRepository.SaveAsync(Arg.Any<Domain.Aggregates.QsoAggregate>()).Returns(LanguageExt.Unit.Default);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify that the new participant gets order 2
        var reader = _mockChannel.Reader;
        reader.TryRead(out var dispatchedEvent).Should().BeTrue();
        
        var participantAddedEvent = dispatchedEvent.Should().BeOfType<Domain.Aggregates.QsoAggregate.Events.ParticipantAdded>().Subject;
        participantAddedEvent.AggregateId.Should().Be(aggregateId);
        participantAddedEvent.CallSign.Should().Be(callSign);
        participantAddedEvent.Order.Should().Be(2); // Should be order 2 since there's already one participant
    }

    [Fact]
    public async Task Handle_WhenDomainErrorOccurs_ShouldNotDispatchEvents()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = ""; // Invalid call sign to trigger domain error
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        var aggregate = CreateQsoAggregate(aggregateId, userId);
        _mockRepository.GetByIdAsync(aggregateId).Returns(aggregate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        
        // Verify that no events were dispatched to the channel
        var reader = _mockChannel.Reader;
        reader.TryRead(out _).Should().BeFalse("No events should be dispatched when domain error occurs");
    }

    [Fact]
    public async Task Handle_WhenRepositoryErrorOccurs_ShouldNotDispatchEvents()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var callSign = "F4TEST";
        
        var user = CreateClaimsPrincipal(userId);
        var command = new AddParticipantCommand(aggregateId, callSign, user);

        _mockRepository.GetByIdAsync(aggregateId).Returns(Error.New("Aggregate not found"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        
        // Verify that no events were dispatched to the channel
        var reader = _mockChannel.Reader;
        reader.TryRead(out _).Should().BeFalse("No events should be dispatched when repository error occurs");
    }

    [Fact]
    public async Task Handle_WhenSaveFailsAfterAddingParticipant_ShouldNotDispatchEvents()
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
        var result = await _handler.Handle(command, CancellationToken.None);        // Assert
        result.IsFail.Should().BeTrue();
        
        // Verify that no events were dispatched to the channel even though the participant was added to the aggregate
        var reader = _mockChannel.Reader;
        reader.TryRead(out _).Should().BeFalse("No events should be dispatched when save operation fails");
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
