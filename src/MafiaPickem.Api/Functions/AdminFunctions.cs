using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class AdminFunctions
{
    private readonly IMatchRepository _matchRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IMatchStateService _matchStateService;
    private readonly IScoringService _scoringService;
    private readonly IStatePublishService _statePublishService;
    private readonly IUserContext _userContext;

    public AdminFunctions(
        IMatchRepository matchRepository,
        ITournamentRepository tournamentRepository,
        IPredictionRepository predictionRepository,
        IMatchStateService matchStateService,
        IScoringService scoringService,
        IStatePublishService statePublishService,
        IUserContext userContext)
    {
        _matchRepository = matchRepository;
        _tournamentRepository = tournamentRepository;
        _predictionRepository = predictionRepository;
        _matchStateService = matchStateService;
        _scoringService = scoringService;
        _statePublishService = statePublishService;
        _userContext = userContext;
    }

    [Function("AdminCreateTournament")]
    public async Task<HttpResponseData> CreateTournamentHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/tournaments")] HttpRequestData req)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var request = await req.ReadFromJsonAsync<CreateTournamentRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Tournament name is required");
            return badRequestResponse;
        }

        var tournament = await _tournamentRepository.CreateAsync(
            request.Name,
            request.Description,
            request.ImageUrl);

        var dto = new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Description = tournament.Description,
            ImageUrl = tournament.ImageUrl
        };

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(dto);
        return response;
    }

    [Function("AdminCreateMatch")]
    public async Task<HttpResponseData> CreateMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/matches")] HttpRequestData req)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var request = await req.ReadFromJsonAsync<CreateMatchRequest>();
        if (request == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid request body");
            return badRequestResponse;
        }

        var match = await _matchRepository.CreateAsync(
            request.TournamentId,
            request.GameNumber,
            request.TableNumber,
            request.ExternalMatchRef);

        // Publish initial blob state so polling clients can discover this match
        await _statePublishService.PublishMatchStateAsync(match.Id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State
        };

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminOpenMatch")]
    public async Task<HttpResponseData> OpenMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/open-match/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var match = await _matchStateService.OpenMatchAsync(id);
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminRevertToUpcoming")]
    public async Task<HttpResponseData> RevertToUpcomingHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/revert-to-upcoming/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var match = await _matchStateService.RevertToUpcomingAsync(id);
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminLockMatch")]
    public async Task<HttpResponseData> LockMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/lock-match/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var match = await _matchStateService.LockMatchAsync(id);
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminResolveMatch")]
    public async Task<HttpResponseData> ResolveMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/resolve-match/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var request = await req.ReadFromJsonAsync<ResolveMatchRequest>();
        if (request == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid request body");
            return badRequestResponse;
        }

        // Get match for tournament ID
        var match = await _matchRepository.GetByIdAsync(id);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match {id} not found");
            return notFoundResponse;
        }

        // Sort voted out slots and convert to CSV
        var sortedSlots = request.VotedOutSlots.OrderBy(s => s).ToList();
        var correctVotedOutCsv = string.Join(",", sortedSlots);

        // Save match result
        await _predictionRepository.SaveMatchResultAsync(id, request.WinningSide, correctVotedOutCsv);

        // Get correct vote counts
        var correctWinnerVotes = await _predictionRepository.GetCorrectWinnerVotesAsync(id, request.WinningSide);
        var correctVotedOutVotes = await _predictionRepository.GetCorrectVotedOutVotesAsync(id, correctVotedOutCsv);

        // Calculate and save scores
        await _scoringService.CalculateAndSaveScoresAsync(id, match.TournamentId, correctWinnerVotes, correctVotedOutVotes);

        // Transition state to Resolved
        var resolvedMatch = await _matchStateService.ResolveMatchAsync(id);

        // Publish state to blob
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = resolvedMatch.Id,
            GameNumber = resolvedMatch.GameNumber,
            TableNumber = resolvedMatch.TableNumber,
            State = resolvedMatch.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminCancelMatch")]
    public async Task<HttpResponseData> CancelMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/cancel-match/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        // Transition state to Canceled
        var match = await _matchStateService.CancelMatchAsync(id);

        // Delete any scores that may exist
        await _predictionRepository.DeleteScoresByMatchIdAsync(id);

        // Publish state to blob
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = match.Id,
            GameNumber = match.GameNumber,
            TableNumber = match.TableNumber,
            State = match.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminPublishState")]
    public async Task<HttpResponseData> PublishStateHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/publish-match-state/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("State published successfully");
        return response;
    }

    [Function("AdminGetTournamentStats")]
    public async Task<HttpResponseData> GetTournamentStatsHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/tournament-stats/{id}")] HttpRequestData req,
        int id)
    {
        if (!_userContext.IsAdmin)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteStringAsync("Admin access required");
            return forbiddenResponse;
        }

        var matches = await _matchRepository.GetByTournamentAndStateAsync(
            id,
            MatchState.Upcoming,
            MatchState.Open,
            MatchState.Locked,
            MatchState.Resolved,
            MatchState.Canceled);

        var matchesList = matches.ToList();

        var stats = new
        {
            TotalMatches = matchesList.Count,
            UpcomingMatches = matchesList.Count(m => m.State == MatchState.Upcoming),
            OpenMatches = matchesList.Count(m => m.State == MatchState.Open),
            LockedMatches = matchesList.Count(m => m.State == MatchState.Locked),
            ResolvedMatches = matchesList.Count(m => m.State == MatchState.Resolved),
            CanceledMatches = matchesList.Count(m => m.State == MatchState.Canceled),
            TotalPredictions = 0
        };

        // Get total predictions across all matches
        int totalPredictions = 0;
        foreach (var match in matchesList)
        {
            if (match.State >= MatchState.Open)
            {
                totalPredictions += await _predictionRepository.GetTotalVotesAsync(match.Id);
            }
        }

        var finalStats = new
        {
            stats.TotalMatches,
            stats.UpcomingMatches,
            stats.OpenMatches,
            stats.LockedMatches,
            stats.ResolvedMatches,
            stats.CanceledMatches,
            TotalPredictions = totalPredictions
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(finalStats);
        return response;
    }
}
