using MafiaPickem.Api.Models.Responses;

namespace MafiaPickem.Api.Data;

public interface ILeaderboardRepository
{
    Task UpdateLeaderboardAsync(int tournamentId);
    Task<LeaderboardResponse> GetLeaderboardAsync(int tournamentId, int currentUserId);
}
