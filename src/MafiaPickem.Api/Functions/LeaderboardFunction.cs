using MafiaPickem.Api.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class LeaderboardFunction
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly ILogger<LeaderboardFunction> _logger;

    public LeaderboardFunction(
        ILeaderboardRepository leaderboardRepository,
        ILogger<LeaderboardFunction>? logger = null)
    {
        _leaderboardRepository = leaderboardRepository;
        _logger = logger ?? null!;
    }

    [Function("GetLeaderboard")]
    public async Task<HttpResponseData> GetLeaderboardHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tournaments/{id}/leaderboard")] HttpRequestData req,
        int id)
    {
        try
        {
            var leaderboard = await _leaderboardRepository.GetLeaderboardAsync(id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(leaderboard);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting leaderboard for tournament {TournamentId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }
}
