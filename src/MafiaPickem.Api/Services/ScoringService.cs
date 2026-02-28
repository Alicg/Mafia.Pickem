using MafiaPickem.Api.Data;
using MafiaPickem.Api.State;

namespace MafiaPickem.Api.Services;

public class ScoringService : IScoringService
{
    private readonly IPredictionRepository _predictionRepository;
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILeaderboardBlobWriter _leaderboardBlobWriter;

    public ScoringService(
        IPredictionRepository predictionRepository,
        ILeaderboardRepository leaderboardRepository,
        IMatchRepository matchRepository,
        ILeaderboardBlobWriter leaderboardBlobWriter)
    {
        _predictionRepository = predictionRepository;
        _leaderboardRepository = leaderboardRepository;
        _matchRepository = matchRepository;
        _leaderboardBlobWriter = leaderboardBlobWriter;
    }

    public async Task CalculateAndSaveScoresAsync(int matchId, int tournamentId, int correctWinnerVotes, int correctVotedOutVotes)
    {
        // Get total votes for the match
        var totalVotes = await _predictionRepository.GetTotalVotesAsync(matchId);

        // Insert scores for all predictions of this match
        await _predictionRepository.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes);

        // Update leaderboard for the tournament
        await _leaderboardRepository.UpdateLeaderboardAsync(tournamentId);

        // Publish leaderboard blob
        await PublishLeaderboardBlobAsync(tournamentId);
    }

    public async Task RollbackScoresAsync(int matchId, int tournamentId)
    {
        // Delete prediction scores for this match
        await _predictionRepository.DeleteScoresByMatchIdAsync(matchId);

        // Delete match result
        await _predictionRepository.DeleteMatchResultByMatchIdAsync(matchId);

        // Recalculate leaderboard without this match's scores
        await _leaderboardRepository.UpdateLeaderboardAsync(tournamentId);

        // Publish updated leaderboard blob
        await PublishLeaderboardBlobAsync(tournamentId);
    }

    private async Task PublishLeaderboardBlobAsync(int tournamentId)
    {
        var leaderboard = await _leaderboardRepository.GetLeaderboardAsync(tournamentId);
        await _leaderboardBlobWriter.WriteAsync(tournamentId, leaderboard);
    }
}
