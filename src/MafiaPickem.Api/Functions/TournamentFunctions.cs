using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class TournamentFunctions
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly ITournamentParticipantRepository _tournamentParticipantRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<TournamentFunctions> _logger;

    public TournamentFunctions(
        ITournamentRepository tournamentRepository,
        IMatchRepository matchRepository,
        IPredictionRepository predictionRepository,
        ITournamentParticipantRepository tournamentParticipantRepository,
        IUserContext userContext,
        ILogger<TournamentFunctions>? logger = null)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _predictionRepository = predictionRepository;
        _tournamentParticipantRepository = tournamentParticipantRepository;
        _userContext = userContext;
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
                var selectedTeamName = await GetSelectedTeamNameAsync(tournament.Id);
                tournamentDtos.Add(MapToDto(tournament, currentMatch, selectedTeamName));
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
            var selectedTeamName = await GetSelectedTeamNameAsync(tournament.Id);
            var tournamentDto = MapToDto(tournament, currentMatch, selectedTeamName);

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

    [Function("GetMyPredictions")]
    public async Task<HttpResponseData> GetMyPredictionsHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tournaments/{id}/my-predictions")] HttpRequestData req,
        int id)
    {
        try
        {
            if (!_userContext.IsRegistered)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new Dictionary<string, PredictionDto>());
                return response;
            }

            var predictions = await _predictionRepository.GetByTournamentAndUserAsync(id, _userContext.UserId);
            var result = new Dictionary<string, PredictionDto>();

            foreach (var p in predictions)
            {
                var dto = new PredictionDto
                {
                    PredictedWinner = p.PredictedWinner,
                    PredictedVotedOut = p.PredictedVotedOut,
                    PredictedLastRound = p.PredictedLastRound
                };

                // Load scores for resolved matches
                var score = await _predictionRepository.GetScoreByPredictionIdAsync(p.Id);
                if (score != null)
                {
                    dto.WinnerPoints = score.WinnerPoints;
                    dto.VotedOutPoints = score.VotedOutPoints;
                    dto.LastRoundPoints = score.LastRoundPoints;
                    dto.TotalPoints = score.TotalPoints;
                }

                result[p.MatchId.ToString()] = dto;
            }

            var resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(result);
            return resp;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting predictions for tournament {TournamentId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    [Function("SelectTournamentTeam")]
    public async Task<HttpResponseData> SelectTournamentTeamHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tournaments/{id}/team")] HttpRequestData req,
        int id)
    {
        try
        {
            if (!_userContext.IsRegistered)
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("User must be registered");
                return unauthorizedResponse;
            }

            var tournament = await _tournamentRepository.GetByIdAsync(id);
            if (tournament == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Tournament with ID {id} not found");
                return notFoundResponse;
            }

            var teams = ParseTeams(tournament.TeamsJson);
            if (teams.Count == 0)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("This tournament does not have configured teams");
                return badRequestResponse;
            }

            var existingSelection = await _tournamentParticipantRepository.GetByTournamentAndUserAsync(id, _userContext.UserId);
            if (existingSelection != null)
            {
                var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                await conflictResponse.WriteStringAsync("Team selection is already locked for this tournament");
                return conflictResponse;
            }

            var request = await req.ReadFromJsonAsync<SelectTournamentTeamRequest>();
            var requestedTeam = request?.TeamName?.Trim();
            if (string.IsNullOrWhiteSpace(requestedTeam))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Team name is required");
                return badRequestResponse;
            }

            var matchedTeam = teams.FirstOrDefault(team => string.Equals(team, requestedTeam, StringComparison.OrdinalIgnoreCase));
            if (matchedTeam == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Selected team is not available for this tournament");
                return badRequestResponse;
            }

            var participant = await _tournamentParticipantRepository.CreateAsync(id, _userContext.UserId, matchedTeam);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new TournamentTeamSelectionDto
            {
                TournamentId = participant.TournamentId,
                TeamName = participant.TeamName
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error selecting team for tournament {TournamentId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    private async Task<string?> GetSelectedTeamNameAsync(int tournamentId)
    {
        if (!_userContext.IsRegistered)
        {
            return null;
        }

        var participant = await _tournamentParticipantRepository.GetByTournamentAndUserAsync(tournamentId, _userContext.UserId);
        return participant?.TeamName;
    }

    private static TournamentDto MapToDto(Tournament tournament, Match? currentMatch, string? selectedTeamName)
    {
        return new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Description = tournament.Description,
            ImageUrl = tournament.ImageUrl,
            Teams = ParseTeams(tournament.TeamsJson),
            SelectedTeamName = selectedTeamName,
            CurrentMatch = currentMatch != null ? MapMatchToDto(currentMatch) : null
        };
    }

    private static List<string> ParseTeams(string? teamsJson)
    {
        if (string.IsNullOrWhiteSpace(teamsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(teamsJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static MatchDto MapMatchToDto(Match match)
    {
        return new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State
        };
    }
}
