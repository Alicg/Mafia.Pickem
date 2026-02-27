using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Responses;

namespace MafiaPickem.Api.Data;

public interface IPredictionRepository
{
    Task<Prediction?> GetByMatchAndUserAsync(int matchId, int userId);
    Task UpsertAsync(int matchId, int userId, byte predictedWinner, byte predictedVotedOut);
    Task<VoteStatsDto> GetVoteStatsAsync(int matchId);
    Task<int> GetTotalVotesAsync(int matchId);
    Task<int> GetCorrectWinnerVotesAsync(int matchId, byte winningSide);
    Task<int> GetCorrectVotedOutVotesAsync(int matchId, string correctVotedOutCsv);
    Task InsertScoresAsync(int matchId, int totalVotes, int correctWinnerVotes, int correctVotedOutVotes);
    Task<PredictionScore?> GetScoreByPredictionIdAsync(int predictionId);
    Task SaveMatchResultAsync(int matchId, byte winningSide, string correctVotedOutCsv);
    Task<(byte WinningSide, string CorrectVotedOutCsv)?> GetMatchResultAsync(int matchId);
    Task DeleteScoresByMatchIdAsync(int matchId);
}
