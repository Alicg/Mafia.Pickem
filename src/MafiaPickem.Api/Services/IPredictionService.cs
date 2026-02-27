namespace MafiaPickem.Api.Services;

public interface IPredictionService
{
    Task SubmitPredictionAsync(int matchId, int userId, byte predictedWinner, byte predictedVotedOut);
}
