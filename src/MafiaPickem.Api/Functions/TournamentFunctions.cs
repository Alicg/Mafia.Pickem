using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class TournamentFunctions
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<TournamentFunctions> _logger;

    public TournamentFunctions(
        ITournamentRepository tournamentRepository,
        IMatchRepository matchRepository,
        ILogger<TournamentFunctions>? logger = null)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _logger = logger ?? null!;
    }

    [Function("GetActiveTournaments")]
    public async Task<HttpResponseData> GetActiveTournamentsHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tournaments/active")] HttpRequestData req)
    {
        try
        {
            var tournaments = await _tournamentRepository.GetActiveAsync();
            var tournamentDtos = new List<TournamentDto>();

            foreach (var tournament in tournaments)
            {
                var currentMatch = await _matchRepository.GetCurrentMatchByTournamentIdAsync(tournament.Id);
                tournamentDtos.Add(MapToDto(tournament, currentMatch));
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(tournamentDtos);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting active tournaments");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    [Function("GetTournament")]
    public async Task<HttpResponseData> GetTournamentHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tournaments/{id}")] HttpRequestData req,
        int id)
    {
        try
        {
            var tournament = await _tournamentRepository.GetByIdAsync(id);
            if (tournament == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Tournament with ID {id} not found");
                return notFoundResponse;
            }

            var currentMatch = await _matchRepository.GetCurrentMatchByTournamentIdAsync(tournament.Id);
            var tournamentDto = MapToDto(tournament, currentMatch);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(tournamentDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting tournament {TournamentId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    [Function("GetTournamentMatches")]
    public async Task<HttpResponseData> GetTournamentMatchesHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tournaments/{id}/matches")] HttpRequestData req,
        int id)
    {
        try
        {
            var matches = await _matchRepository.GetByTournamentIdAsync(id);
            var matchDtos = matches.Select(MapMatchToDto).ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(matchDtos);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting matches for tournament {TournamentId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    private static TournamentDto MapToDto(Tournament tournament, Match? currentMatch)
    {
        return new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Description = tournament.Description,
            ImageUrl = tournament.ImageUrl,
            CurrentMatch = currentMatch != null ? MapMatchToDto(currentMatch) : null
        };
    }

    private static MatchDto MapMatchToDto(Match match)
    {
        return new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State,
            MyPrediction = null, // Will be implemented in Phase 4
            VoteStats = null     // Will be implemented in Phase 4
        };
    }
}
