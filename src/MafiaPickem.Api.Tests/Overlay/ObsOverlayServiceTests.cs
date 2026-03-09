using FluentAssertions;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Overlay;
using MafiaPickem.Api.State;
using Moq;
using DomainMatch = MafiaPickem.Api.Models.Domain.Match;

namespace MafiaPickem.Api.Tests.Overlay;

public class ObsOverlayServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IMatchStateBlobReader> _blobReaderMock;
    private readonly ObsOverlayService _service;

    public ObsOverlayServiceTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _blobReaderMock = new Mock<IMatchStateBlobReader>();
        _service = new ObsOverlayService(_matchRepositoryMock.Object, _blobReaderMock.Object);
    }

    [Fact]
    public async Task GetOverlayPayloadAsync_ShouldPreferOpenMatchOverResolved()
    {
        // Arrange
        const int tournamentId = 77;
        var resolvedMatch = new DomainMatch
        {
            Id = 100,
            TournamentId = tournamentId,
            GameNumber = 8,
            TableNumber = 3,
            State = MatchState.Resolved,
            DateResolved = DateTime.UtcNow.AddMinutes(-5),
            DateCreated = DateTime.UtcNow.AddHours(-1)
        };
        var openMatch = new DomainMatch
        {
            Id = 90,
            TournamentId = tournamentId,
            GameNumber = 9,
            TableNumber = 4,
            State = MatchState.Open,
            DateOpened = DateTime.UtcNow.AddMinutes(-1),
            DateCreated = DateTime.UtcNow.AddHours(-2)
        };

        _matchRepositoryMock
            .Setup(repository => repository.GetByTournamentAndStateAsync(tournamentId, MatchState.Open, MatchState.Resolved))
            .ReturnsAsync(new[] { resolvedMatch, openMatch });

        _blobReaderMock
            .Setup(reader => reader.ReadStateAsync(openMatch.Id))
            .ReturnsAsync(new BlobMatchState
            {
                MatchId = openMatch.Id,
                TournamentId = tournamentId,
                State = "Open",
                UpdatedAt = DateTime.UtcNow,
                TotalPredictions = 25,
                WinnerVotes = new WinnerVotesDto
                {
                    Town = new VoteEntry { Count = 16, Percent = 64m },
                    Mafia = new VoteEntry { Count = 9, Percent = 36m }
                },
                VotedOutVotes = new List<SlotVoteEntry>
                {
                    new() { Slot = 4, Count = 6, Percent = 24m },
                    new() { Slot = 9, Count = 8, Percent = 32m }
                }
            });

        // Act
        var payload = await _service.GetOverlayPayloadAsync(tournamentId);

        // Assert
        payload.Status.Should().Be("ready");
        payload.MatchId.Should().Be(openMatch.Id);
        payload.GameNumber.Should().Be(openMatch.GameNumber);
        payload.TableNumber.Should().Be(openMatch.TableNumber);
        payload.MatchState.Should().Be("Open");
        payload.TotalPredictions.Should().Be(25);
        payload.RedSide.Count.Should().Be(16);
        payload.BlackSide.Count.Should().Be(9);
        payload.SeatVotes.Should().HaveCount(10);
        payload.SeatVotes.Single(seat => seat.Slot == 4).Count.Should().Be(6);
        payload.SeatVotes.Single(seat => seat.Slot == 9).Percent.Should().Be(32m);

        _blobReaderMock.Verify(reader => reader.ReadStateAsync(openMatch.Id), Times.Once);
        _blobReaderMock.Verify(reader => reader.ReadStateAsync(resolvedMatch.Id), Times.Never);
    }

    [Fact]
    public async Task GetOverlayPayloadAsync_ShouldFallbackToLatestResolvedMatch()
    {
        // Arrange
        const int tournamentId = 88;
        var olderResolved = new DomainMatch
        {
            Id = 50,
            TournamentId = tournamentId,
            GameNumber = 4,
            TableNumber = 1,
            State = MatchState.Resolved,
            DateResolved = DateTime.UtcNow.AddMinutes(-15),
            DateCreated = DateTime.UtcNow.AddHours(-2)
        };
        var latestResolved = new DomainMatch
        {
            Id = 51,
            TournamentId = tournamentId,
            GameNumber = 5,
            TableNumber = 2,
            State = MatchState.Resolved,
            DateResolved = DateTime.UtcNow.AddMinutes(-2),
            DateCreated = DateTime.UtcNow.AddHours(-1)
        };

        _matchRepositoryMock
            .Setup(repository => repository.GetByTournamentAndStateAsync(tournamentId, MatchState.Open, MatchState.Resolved))
            .ReturnsAsync(new[] { olderResolved, latestResolved });

        _blobReaderMock
            .Setup(reader => reader.ReadStateAsync(latestResolved.Id))
            .ReturnsAsync(new BlobMatchState
            {
                MatchId = latestResolved.Id,
                TournamentId = tournamentId,
                State = "Resolved",
                UpdatedAt = DateTime.UtcNow,
                TotalPredictions = 18,
                WinnerVotes = new WinnerVotesDto
                {
                    Town = new VoteEntry { Count = 7, Percent = 38.9m },
                    Mafia = new VoteEntry { Count = 11, Percent = 61.1m }
                },
                MatchResult = new MatchResultDto
                {
                    WinningSide = 1,
                    VotedOutSlots = new List<int> { 2, 7 }
                }
            });

        // Act
        var payload = await _service.GetOverlayPayloadAsync(tournamentId);

        // Assert
        payload.Status.Should().Be("ready");
        payload.MatchId.Should().Be(latestResolved.Id);
        payload.MatchState.Should().Be("Resolved");
        payload.WinningSide.Should().Be(1);
        payload.ResolvedSlots.Should().Equal(2, 7);
        payload.SeatVotes.Single(seat => seat.Slot == 2).IsResolved.Should().BeTrue();
        payload.SeatVotes.Single(seat => seat.Slot == 7).IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task GetOverlayPayloadAsync_ShouldReturnMissingStateWhenBlobWasNotPublished()
    {
        // Arrange
        const int tournamentId = 91;
        var openMatch = new DomainMatch
        {
            Id = 12,
            TournamentId = tournamentId,
            GameNumber = 6,
            TableNumber = 7,
            State = MatchState.Open,
            DateOpened = DateTime.UtcNow,
            DateCreated = DateTime.UtcNow.AddHours(-1)
        };

        _matchRepositoryMock
            .Setup(repository => repository.GetByTournamentAndStateAsync(tournamentId, MatchState.Open, MatchState.Resolved))
            .ReturnsAsync(new[] { openMatch });
        _blobReaderMock
            .Setup(reader => reader.ReadStateAsync(openMatch.Id))
            .ReturnsAsync((BlobMatchState?)null);

        // Act
        var payload = await _service.GetOverlayPayloadAsync(tournamentId);

        // Assert
        payload.Status.Should().Be("missing-state");
        payload.MatchId.Should().Be(openMatch.Id);
        payload.GameNumber.Should().Be(openMatch.GameNumber);
        payload.TableNumber.Should().Be(openMatch.TableNumber);
        payload.SeatVotes.Should().HaveCount(10);
        payload.TotalPredictions.Should().Be(0);
    }
}
