namespace MafiaPickem.Api.Services;

public interface IScoringService
{
    Task CalculateAndSaveScoresAsync(int matchId, int tournamentId, int correctWinnerVotes, int correctVotedOutVotes);
    Task RollbackScoresAsync(int matchId, int tournamentId);
}
