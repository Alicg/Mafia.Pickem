using FluentAssertions;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Services;
using Moq;
using DomainMatch = MafiaPickem.Api.Models.Domain.Match;

namespace MafiaPickem.Api.Tests.Services;

public class ScoringServiceTests
{
    private readonly Mock<IPredictionRepository> _mockPredictionRepo;
    private readonly Mock<ILeaderboardRepository> _mockLeaderboardRepo;
    private readonly Mock<IMatchRepository> _mockMatchRepo;
    private readonly IScoringService _scoringService;

    public ScoringServiceTests()
    {
        _mockPredictionRepo = new Mock<IPredictionRepository>();
        _mockLeaderboardRepo = new Mock<ILeaderboardRepository>();
        _mockMatchRepo = new Mock<IMatchRepository>();
        _scoringService = new ScoringService(
            _mockPredictionRepo.Object,
            _mockLeaderboardRepo.Object,
            _mockMatchRepo.Object);
    }

    [Fact]
    public async Task CalculateScores_WithAllCorrect_ShouldCalculateMaxPoints()
    {
        // Arrange
        var matchId = 1;
        var tournamentId = 10;
        var totalVotes = 100;
        var correctWinnerVotes = 60;
        var correctVotedOutVotes = 30;

        var match = new DomainMatch { Id = matchId, TournamentId = tournamentId };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepo.Setup(r => r.GetTotalVotesAsync(matchId))
            .ReturnsAsync(totalVotes);
        _mockPredictionRepo.Setup(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes))
            .Returns(Task.CompletedTask);
        _mockLeaderboardRepo.Setup(r => r.UpdateLeaderboardAsync(tournamentId))
            .Returns(Task.CompletedTask);

        // Act
        await _scoringService.CalculateAndSaveScoresAsync(matchId, tournamentId, correctWinnerVotes, correctVotedOutVotes);

        // Assert
        _mockPredictionRepo.Verify(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes), Times.Once);
        _mockLeaderboardRepo.Verify(r => r.UpdateLeaderboardAsync(tournamentId), Times.Once);
    }

    [Fact]
    public async Task CalculateScores_WithPartialCorrect_ShouldCalculatePartialPoints()
    {
        // Arrange
        var matchId = 1;
        var tournamentId = 10;
        var totalVotes = 50;
        var correctWinnerVotes = 20;
        var correctVotedOutVotes = 10;

        var match = new DomainMatch { Id = matchId, TournamentId = tournamentId };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepo.Setup(r => r.GetTotalVotesAsync(matchId))
            .ReturnsAsync(totalVotes);
        _mockPredictionRepo.Setup(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes))
            .Returns(Task.CompletedTask);
        _mockLeaderboardRepo.Setup(r => r.UpdateLeaderboardAsync(tournamentId))
            .Returns(Task.CompletedTask);

        // Act
        await _scoringService.CalculateAndSaveScoresAsync(matchId, tournamentId, correctWinnerVotes, correctVotedOutVotes);

        // Assert
        _mockPredictionRepo.Verify(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes), Times.Once);
    }

    [Fact]
    public async Task CalculateScores_WithNoCorrectWinner_ShouldGiveZeroWinnerPoints()
    {
        // Arrange
        var matchId = 1;
        var tournamentId = 10;
        var totalVotes = 50;
        var correctWinnerVotes = 0;
        var correctVotedOutVotes = 10;

        var match = new DomainMatch { Id = matchId, TournamentId = tournamentId };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepo.Setup(r => r.GetTotalVotesAsync(matchId))
            .ReturnsAsync(totalVotes);
        _mockPredictionRepo.Setup(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes))
            .Returns(Task.CompletedTask);
        _mockLeaderboardRepo.Setup(r => r.UpdateLeaderboardAsync(tournamentId))
            .Returns(Task.CompletedTask);

        // Act
        await _scoringService.CalculateAndSaveScoresAsync(matchId, tournamentId, correctWinnerVotes, correctVotedOutVotes);

        // Assert
        _mockPredictionRepo.Verify(r => r.InsertScoresAsync(matchId, totalVotes, 0, correctVotedOutVotes), Times.Once);
    }

    [Fact]
    public async Task CalculateScores_WithNoCorrectVotedOut_ShouldGiveZeroVotedOutPoints()
    {
        // Arrange
        var matchId = 1;
        var tournamentId = 10;
        var totalVotes = 50;
        var correctWinnerVotes = 20;
        var correctVotedOutVotes = 0;

        var match = new DomainMatch { Id = matchId, TournamentId = tournamentId };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepo.Setup(r => r.GetTotalVotesAsync(matchId))
            .ReturnsAsync(totalVotes);
        _mockPredictionRepo.Setup(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes))
            .Returns(Task.CompletedTask);
        _mockLeaderboardRepo.Setup(r => r.UpdateLeaderboardAsync(tournamentId))
            .Returns(Task.CompletedTask);

        // Act
        await _scoringService.CalculateAndSaveScoresAsync(matchId, tournamentId, correctWinnerVotes, 0);

        // Assert
        _mockPredictionRepo.Verify(r => r.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, 0), Times.Once);
    }

    [Fact]
    public async Task CalculateScores_WithZeroPredictions_ShouldHandleGracefully()
    {
        // Arrange
        var matchId = 1;
        var tournamentId = 10;
        var totalVotes = 0;

        var match = new DomainMatch { Id = matchId, TournamentId = tournamentId };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockPredictionRepo.Setup(r => r.GetTotalVotesAsync(matchId))
            .ReturnsAsync(totalVotes);
        _mockPredictionRepo.Setup(r => r.InsertScoresAsync(matchId, 0, 0, 0))
            .Returns(Task.CompletedTask);
        _mockLeaderboardRepo.Setup(r => r.UpdateLeaderboardAsync(tournamentId))
            .Returns(Task.CompletedTask);

        // Act
        await _scoringService.CalculateAndSaveScoresAsync(matchId, tournamentId, 0, 0);

        // Assert
        _mockPredictionRepo.Verify(r => r.InsertScoresAsync(matchId, 0, 0, 0), Times.Once);
        _mockLeaderboardRepo.Verify(r => r.UpdateLeaderboardAsync(tournamentId), Times.Once);
    }
}
