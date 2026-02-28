using MafiaPickem.Api.Data;

namespace MafiaPickem.Api.Services;

public class ScoringService : IScoringService
{
    private readonly IPredictionRepository _predictionRepository;
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IMatchRepository _matchRepository;

    public ScoringService(
        IPredictionRepository predictionRepository,
        ILeaderboardRepository leaderboardRepository,
        IMatchRepository matchRepository)
    {
        _predictionRepository = predictionRepository;
        _leaderboardRepository = leaderboardRepository;
        _matchRepository = matchRepository;
    }

    public async Task CalculateAndSaveScoresAsync(int matchId, int tournamentId, int correctWinnerVotes, int correctVotedOutVotes)
    {
        // Get total votes for the match
        var totalVotes = await _predictionRepository.GetTotalVotesAsync(matchId);

        // Insert scores for all predictions of this match
        await _predictionRepository.InsertScoresAsync(matchId, totalVotes, correctWinnerVotes, correctVotedOutVotes);

        // Update leaderboard for the tournament
        await _leaderboardRepository.UpdateLeaderboardAsync(tournamentId);
    }

    public async Task RollbackScoresAsync(int matchId, int tournamentId)
    {
        // Delete prediction scores for this match
        await _predictionRepository.DeleteScoresByMatchIdAsync(matchId);

        // Delete match result
        await _predictionRepository.DeleteMatchResultByMatchIdAsync(matchId);

        // Recalculate leaderboard without this match's scores
        await _leaderboardRepository.UpdateLeaderboardAsync(tournamentId);
    }
}
