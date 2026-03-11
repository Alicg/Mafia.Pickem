using FluentAssertions;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Services;
using Moq;
using DomainMatch = MafiaPickem.Api.Models.Domain.Match;

namespace MafiaPickem.Api.Tests.Services;

public class MatchStateServiceTests
{
    private readonly Mock<IMatchRepository> _mockRepository;
    private readonly IMatchStateService _matchStateService;

    public MatchStateServiceTests()
    {
        _mockRepository = new Mock<IMatchRepository>();
        _matchStateService = new MatchStateService(_mockRepository.Object);
    }

    [Fact]
    public async Task OpenMatch_FromUpcoming_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = MatchState.Upcoming };
        var updatedMatch = new DomainMatch { Id = matchId, State = MatchState.Open };

        _mockRepository.SetupSequence(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match)
            .ReturnsAsync(updatedMatch);
        _mockRepository.Setup(r => r.UpdateStateAsync(matchId, MatchState.Open))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _matchStateService.OpenMatchAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateStateAsync(matchId, MatchState.Open), Times.Once);
    }

    [Theory]
    [InlineData(MatchState.Locked)]
    [InlineData(MatchState.Resolved)]
    [InlineData(MatchState.Canceled)]
    public async Task OpenMatch_FromInvalidState_ShouldThrow(MatchState currentState)
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = currentState };

        _mockRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);

        // Act
        var act = async () => await _matchStateService.OpenMatchAsync(matchId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot transition match from {currentState} to Open");
    }

    [Fact]
    public async Task LockMatch_FromOpen_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = MatchState.Open };
        var updatedMatch = new DomainMatch { Id = matchId, State = MatchState.Locked };

        _mockRepository.SetupSequence(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match)
            .ReturnsAsync(updatedMatch);
        _mockRepository.Setup(r => r.UpdateStateAsync(matchId, MatchState.Locked))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _matchStateService.LockMatchAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateStateAsync(matchId, MatchState.Locked), Times.Once);
    }

    [Theory]
    [InlineData(MatchState.Upcoming)]
    [InlineData(MatchState.Resolved)]
    [InlineData(MatchState.Canceled)]
    public async Task LockMatch_FromInvalidState_ShouldThrow(MatchState currentState)
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = currentState };

        _mockRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);

        // Act
        var act = async () => await _matchStateService.LockMatchAsync(matchId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot transition match from {currentState} to Locked");
    }

    [Fact]
    public async Task ResolveMatch_FromLocked_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = MatchState.Locked };
        var updatedMatch = new DomainMatch { Id = matchId, State = MatchState.Resolved };

        _mockRepository.SetupSequence(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match)
            .ReturnsAsync(updatedMatch);
        _mockRepository.Setup(r => r.UpdateStateAsync(matchId, MatchState.Resolved))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _matchStateService.ResolveMatchAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateStateAsync(matchId, MatchState.Resolved), Times.Once);
    }

    [Theory]
    [InlineData(MatchState.Upcoming)]
    [InlineData(MatchState.Open)]
    [InlineData(MatchState.Canceled)]
    public async Task ResolveMatch_FromInvalidState_ShouldThrow(MatchState currentState)
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = currentState };

        _mockRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);

        // Act
        var act = async () => await _matchStateService.ResolveMatchAsync(matchId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot transition match from {currentState} to Resolved");
    }

    [Fact]
    public async Task ResolveMatch_FromFirstVoted_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = MatchState.FirstVoted };
        var updatedMatch = new DomainMatch { Id = matchId, State = MatchState.Resolved };

        _mockRepository.SetupSequence(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match)
            .ReturnsAsync(updatedMatch);
        _mockRepository.Setup(r => r.UpdateStateAsync(matchId, MatchState.Resolved))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _matchStateService.ResolveMatchAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateStateAsync(matchId, MatchState.Resolved), Times.Once);
    }

    [Fact]
    public async Task SetFirstVoted_FromLocked_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = MatchState.Locked };
        var updatedMatch = new DomainMatch { Id = matchId, State = MatchState.FirstVoted };

        _mockRepository.SetupSequence(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match)
            .ReturnsAsync(updatedMatch);
        _mockRepository.Setup(r => r.UpdateStateAsync(matchId, MatchState.FirstVoted))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _matchStateService.SetFirstVotedAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateStateAsync(matchId, MatchState.FirstVoted), Times.Once);
    }

    [Theory]
    [InlineData(MatchState.Upcoming)]
    [InlineData(MatchState.Open)]
    [InlineData(MatchState.Resolved)]
    [InlineData(MatchState.Canceled)]
    public async Task SetFirstVoted_FromInvalidState_ShouldThrow(MatchState currentState)
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = currentState };

        _mockRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);

        // Act
        var act = async () => await _matchStateService.SetFirstVotedAsync(matchId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot transition match from {currentState} to FirstVoted");
    }

    [Fact]
    public async Task UndoFirstVoted_FromFirstVoted_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = MatchState.FirstVoted };
        var updatedMatch = new DomainMatch { Id = matchId, State = MatchState.Locked };

        _mockRepository.SetupSequence(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match)
            .ReturnsAsync(updatedMatch);
        _mockRepository.Setup(r => r.UpdateStateAsync(matchId, MatchState.Locked))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _matchStateService.UndoFirstVotedAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateStateAsync(matchId, MatchState.Locked), Times.Once);
    }

    [Theory]
    [InlineData(MatchState.Upcoming)]
    [InlineData(MatchState.Open)]
    [InlineData(MatchState.Locked)]
    [InlineData(MatchState.Resolved)]
    public async Task UndoFirstVoted_FromInvalidState_ShouldThrow(MatchState currentState)
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch { Id = matchId, State = currentState };

        _mockRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);

        // Act
        var act = async () => await _matchStateService.UndoFirstVotedAsync(matchId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task OpenMatch_WhenMatchNotFound_ShouldThrow()
    {
        // Arrange
        var matchId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync((DomainMatch?)null);

        // Act
        var act = async () => await _matchStateService.OpenMatchAsync(matchId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Match with ID {matchId} not found");
    }
}
