using MafiaPickem.Api.Models.Responses;

namespace MafiaPickem.Api.State;

public interface ILeaderboardBlobWriter
{
    Task WriteAsync(int tournamentId, LeaderboardResponse leaderboard);
}
