using FluentAssertions;
using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Services;
using Moq;
using DomainMatch = MafiaPickem.Api.Models.Domain.Match;

namespace MafiaPickem.Api.Tests.Services;

public class PredictionServiceTests
{
    private readonly Mock<IPredictionRepository> _mockPredictionRepo;
    private readonly Mock<IMatchRepository> _mockMatchRepo;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly IPredictionService _predictionService;

    public PredictionServiceTests()
    {
        _mockPredictionRepo = new Mock<IPredictionRepository>();
        _mockMatchRepo = new Mock<IMatchRepository>();
        _mockUserContext = new Mock<IUserContext>();
        _predictionService = new PredictionService(
            _mockPredictionRepo.Object,
            _mockMatchRepo.Object,
            _mockUserContext.Object);
    }

    [Fact]
    public async Task SubmitPrediction_WithValidData_ShouldSucceed()
    {
        // Arrange
        var matchId = 1;
        var userId = 100;
        byte predictedWinner = 0;
        byte predictedVotedOut = 5;
        var match = new DomainMatch { Id = matchId, State = MatchState.Open };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockUserContext.Setup(u => u.IsRegistered).Returns(true);
        _mockPredictionRepo.Setup(r => r.UpsertAsync(matchId, userId, predictedWinner, predictedVotedOut))
            .Returns(Task.CompletedTask);

        // Act
        await _predictionService.SubmitPredictionAsync(matchId, userId, predictedWinner, predictedVotedOut);

        // Assert
        _mockPredictionRepo.Verify(r => r.UpsertAsync(matchId, userId, predictedWinner, predictedVotedOut), Times.Once);
    }

    [Theory]
    [InlineData(MatchState.Upcoming)]
    [InlineData(MatchState.Locked)]
    [InlineData(MatchState.Resolved)]
    [InlineData(MatchState.Canceled)]
    public async Task SubmitPrediction_WhenMatchNotOpen_ShouldThrow(MatchState state)
    {
        // Arrange
        var matchId = 1;
        var userId = 100;
        var match = new DomainMatch { Id = matchId, State = state };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockUserContext.Setup(u => u.IsRegistered).Returns(true);

        // Act
        var act = async () => await _predictionService.SubmitPredictionAsync(matchId, userId, 0, 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Match is not open for predictions");
    }

    [Fact]
    public async Task SubmitPrediction_WhenMatchNotFound_ShouldThrow()
    {
        // Arrange
        var matchId = 999;
        var userId = 100;

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync((DomainMatch?)null);
        _mockUserContext.Setup(u => u.IsRegistered).Returns(true);

        // Act
        var act = async () => await _predictionService.SubmitPredictionAsync(matchId, userId, 0, 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Match with ID {matchId} not found");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(255)]
    public async Task SubmitPrediction_WithInvalidWinner_ShouldThrow(byte invalidWinner)
    {
        // Arrange
        var matchId = 1;
        var userId = 100;
        var match = new DomainMatch { Id = matchId, State = MatchState.Open };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockUserContext.Setup(u => u.IsRegistered).Returns(true);

        // Act
        var act = async () => await _predictionService.SubmitPredictionAsync(matchId, userId, invalidWinner, 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("PredictedWinner must be 0 (Town) or 1 (Mafia)");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(255)]
    public async Task SubmitPrediction_WithInvalidVotedOut_ShouldThrow(byte invalidVotedOut)
    {
        // Arrange
        var matchId = 1;
        var userId = 100;
        var match = new DomainMatch { Id = matchId, State = MatchState.Open };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockUserContext.Setup(u => u.IsRegistered).Returns(true);

        // Act
        var act = async () => await _predictionService.SubmitPredictionAsync(matchId, userId, 0, invalidVotedOut);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("PredictedVotedOut must be between 0 and 10");
    }

    [Fact]
    public async Task SubmitPrediction_WhenUserNotRegistered_ShouldThrow()
    {
        // Arrange
        var matchId = 1;
        var userId = 100;
        var match = new DomainMatch { Id = matchId, State = MatchState.Open };

        _mockMatchRepo.Setup(r => r.GetByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockUserContext.Setup(u => u.IsRegistered).Returns(false);

        // Act
        var act = async () => await _predictionService.SubmitPredictionAsync(matchId, userId, 0, 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User must be registered to submit predictions");
    }
}
