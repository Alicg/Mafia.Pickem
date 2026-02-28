using FluentAssertions;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using MafiaPickem.Api.State;
using Moq;
using DomainMatch = MafiaPickem.Api.Models.Domain.Match;

namespace MafiaPickem.Api.Tests.Services;

public class StatePublishServiceTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IPredictionRepository> _mockPredictionRepository;
    private readonly Mock<IMatchStateBlobWriter> _mockBlobWriter;
    private readonly IStatePublishService _statePublishService;

    public StatePublishServiceTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockPredictionRepository = new Mock<IPredictionRepository>();
        _mockBlobWriter = new Mock<IMatchStateBlobWriter>();
        _statePublishService = new StatePublishService(
            _mockMatchRepository.Object,
            _mockPredictionRepository.Object,
            _mockBlobWriter.Object);
    }

    [Fact]
    public async Task PublishMatchState_ShouldBuildCorrectBlobState()
    {
        // Arrange
        var matchId = 1;
        var match = new DomainMatch
        {
            Id = matchId,
            TournamentId = 10,
            State = MatchState.Open,
            GameNumber = 3,
            TableNumber = 2
        };

        var voteStats = new VoteStatsDto
        {
            TotalVotes = 20,
            TownPercentage = 60.0m,
            MafiaPercentage = 40.0m,
            SlotVotes = new List<SlotVoteDto>
            {
                new() { Slot = 1, Count = 5, Percentage = 25.0m },
                new() { Slot = 3, Count = 10, Percentage = 50.0m }
            }
        };

        _mockMatchRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepository.Setup(r => r.GetVoteStatsAsync(matchId))
            .ReturnsAsync(voteStats);

        BlobMatchState? capturedState = null;
        _mockBlobWriter.Setup(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()))
            .Callback<BlobMatchState>(s => capturedState = s)
            .Returns(Task.CompletedTask);

        // Act
        await _statePublishService.PublishMatchStateAsync(matchId);

        // Assert
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Once);
        capturedState.Should().NotBeNull();

        capturedState!.MatchId.Should().Be(matchId);
        capturedState.TournamentId.Should().Be(10);
        capturedState.State.Should().Be("Open");
        capturedState.Version.Should().BeGreaterThan(0);
        capturedState.TableSize.Should().Be(10);
        capturedState.TotalPredictions.Should().Be(20);
        capturedState.WinnerVotes.Should().NotBeNull();
        capturedState.WinnerVotes!.Town.Count.Should().Be(12); // 60% of 20
        capturedState.WinnerVotes.Town.Percent.Should().Be(60.0m);
        capturedState.WinnerVotes.Mafia.Count.Should().Be(8); // 40% of 20
        capturedState.WinnerVotes.Mafia.Percent.Should().Be(40.0m);
        capturedState.VotedOutVotes.Should().HaveCount(2);
        capturedState.VotedOutVotes![0].Slot.Should().Be(1);
        capturedState.VotedOutVotes[0].Count.Should().Be(5);
        capturedState.VotedOutVotes[0].Percent.Should().Be(25.0m);
    }

    [Fact]
    public async Task PublishMatchState_InOpenState_ShouldThrottleTo10Seconds()
    {
        // Arrange
        var matchId = 2;
        var match = new DomainMatch
        {
            Id = matchId,
            TournamentId = 10,
            State = MatchState.Open,
            GameNumber = 1
        };

        var voteStats = new VoteStatsDto { TotalVotes = 5 };

        _mockMatchRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepository.Setup(r => r.GetVoteStatsAsync(matchId))
            .ReturnsAsync(voteStats);
        _mockBlobWriter.Setup(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()))
            .Returns(Task.CompletedTask);

        // First call: no previous publish → should publish
        _mockBlobWriter.Setup(w => w.GetLastPublishTimeAsync(matchId))
            .ReturnsAsync((DateTime?)null);

        await _statePublishService.PublishMatchStateAsync(matchId);
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Once);

        // Second call: last publish was 2 seconds ago → should be throttled
        _mockBlobWriter.Setup(w => w.GetLastPublishTimeAsync(matchId))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-2));

        await _statePublishService.PublishMatchStateAsync(matchId);
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Once); // Still once

        // Third call: last publish was 11 seconds ago → should publish
        _mockBlobWriter.Setup(w => w.GetLastPublishTimeAsync(matchId))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-11));

        await _statePublishService.PublishMatchStateAsync(matchId);
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Exactly(2));
    }

    [Fact]
    public async Task PublishMatchState_WithForcePublish_ShouldBypassThrottle()
    {
        // Arrange
        var matchId = 3;
        var match = new DomainMatch
        {
            Id = matchId,
            TournamentId = 10,
            State = MatchState.Open,
            GameNumber = 1
        };

        var voteStats = new VoteStatsDto { TotalVotes = 5 };

        _mockMatchRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepository.Setup(r => r.GetVoteStatsAsync(matchId))
            .ReturnsAsync(voteStats);
        _mockBlobWriter.Setup(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()))
            .Returns(Task.CompletedTask);

        // First call - no previous publish
        _mockBlobWriter.Setup(w => w.GetLastPublishTimeAsync(matchId))
            .ReturnsAsync((DateTime?)null);
        await _statePublishService.PublishMatchStateAsync(matchId, forcePublish: false);
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Once);

        // Second call with forcePublish should bypass throttle even if recently published
        _mockBlobWriter.Setup(w => w.GetLastPublishTimeAsync(matchId))
            .ReturnsAsync(DateTime.UtcNow);
        await _statePublishService.PublishMatchStateAsync(matchId, forcePublish: true);
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Exactly(2));
    }

    [Fact]
    public async Task PublishMatchState_InLockedState_ShouldAlwaysPublish()
    {
        // Arrange
        var matchId = 4;
        var match = new DomainMatch
        {
            Id = matchId,
            TournamentId = 10,
            State = MatchState.Locked,
            GameNumber = 1
        };

        var voteStats = new VoteStatsDto { TotalVotes = 10 };

        _mockMatchRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepository.Setup(r => r.GetVoteStatsAsync(matchId))
            .ReturnsAsync(voteStats);
        _mockBlobWriter.Setup(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()))
            .Returns(Task.CompletedTask);

        // Act - Multiple calls should all publish (no throttle in Locked state)
        await _statePublishService.PublishMatchStateAsync(matchId);
        await _statePublishService.PublishMatchStateAsync(matchId);
        await _statePublishService.PublishMatchStateAsync(matchId);

        // Assert
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()), Times.Exactly(3));
    }

    [Fact]
    public async Task PublishMatchState_InUpcomingState_ShouldNotLoadVoteStats()
    {
        // Arrange
        var matchId = 5;
        var match = new DomainMatch
        {
            Id = matchId,
            TournamentId = 10,
            State = MatchState.Upcoming,
            GameNumber = 1
        };

        _mockMatchRepository.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockBlobWriter.Setup(w => w.WriteStateAsync(It.IsAny<BlobMatchState>()))
            .Returns(Task.CompletedTask);

        // Act
        await _statePublishService.PublishMatchStateAsync(matchId);

        // Assert
        _mockPredictionRepository.Verify(r => r.GetVoteStatsAsync(It.IsAny<int>()), Times.Never);
        _mockBlobWriter.Verify(w => w.WriteStateAsync(It.Is<BlobMatchState>(s =>
            s.TotalPredictions == 0 &&
            s.WinnerVotes == null &&
            s.VotedOutVotes == null
        )), Times.Once);
    }
}
