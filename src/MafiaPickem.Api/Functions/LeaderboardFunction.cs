using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class LeaderboardFunction
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<LeaderboardFunction> _logger;

    public LeaderboardFunction(
        ILeaderboardRepository leaderboardRepository,
        IUserContext userContext,
        ILogger<LeaderboardFunction>? logger = null)
    {
        _leaderboardRepository = leaderboardRepository;
        _userContext = userContext;
        _logger = logger ?? null!;
    }

    [Function("GetLeaderboard")]
    public async Task<HttpResponseData> GetLeaderboardHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tournaments/{id}/leaderboard")] HttpRequestData req,
        int id)
    {
        try
        {
            var currentUserId = _userContext.UserId;
            var leaderboard = await _leaderboardRepository.GetLeaderboardAsync(id, currentUserId);

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
