using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using MafiaPickem.Api.State;
using MafiaPickem.Api.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class AdminFunctions
{
    private readonly IMatchRepository _matchRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITournamentOperatorRepository _tournamentOperatorRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IMatchStateService _matchStateService;
    private readonly IScoringService _scoringService;
    private readonly IStatePublishService _statePublishService;
    private readonly IMatchStateBlobWriter _blobWriter;
    private readonly IUserContext _userContext;

    public AdminFunctions(
        IMatchRepository matchRepository,
        ITournamentRepository tournamentRepository,
        ITournamentOperatorRepository tournamentOperatorRepository,
        IPredictionRepository predictionRepository,
        IMatchStateService matchStateService,
        IScoringService scoringService,
        IStatePublishService statePublishService,
        IMatchStateBlobWriter blobWriter,
        IUserContext userContext)
    {
        _matchRepository = matchRepository;
        _tournamentRepository = tournamentRepository;
        _tournamentOperatorRepository = tournamentOperatorRepository;
        _predictionRepository = predictionRepository;
        _matchStateService = matchStateService;
        _scoringService = scoringService;
        _statePublishService = statePublishService;
        _blobWriter = blobWriter;
        _userContext = userContext;
    }

    [Function("AdminCreateTournament")]
    public async Task<HttpResponseData> CreateTournamentHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/tournaments")] HttpRequestData req)
    {
        if (!_userContext.IsAdmin)
        {
            return await CreateForbiddenResponseAsync(req, "Admin access required");
        }

        var request = await req.ReadFromJsonAsync<CreateTournamentRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Tournament name is required");
            return badRequestResponse;
        }

        var teams = request.Teams
            .Where(team => !string.IsNullOrWhiteSpace(team))
            .Select(team => team.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (teams.Count == 0)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("At least one team is required");
            return badRequestResponse;
        }

        var operatorUsernames = request.OperatorUsernames
            .Select(TelegramUsernameNormalizer.NormalizeMention)
            .Where(username => username != null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var tournament = await _tournamentRepository.CreateAsync(
            request.Name,
            request.Description,
            request.ImageUrl,
            teams);

        if (operatorUsernames.Count > 0)
        {
            await _tournamentOperatorRepository.AddRangeAsync(tournament.Id, operatorUsernames);
        }

        var dto = new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Description = tournament.Description,
            ImageUrl = tournament.ImageUrl,
            Teams = teams,
            CanManage = true
        };

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(dto);
        return response;
    }

    [Function("AdminCreateMatch")]
    public async Task<HttpResponseData> CreateMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/matches")] HttpRequestData req)
    {
        if (!_userContext.IsAdmin && string.IsNullOrWhiteSpace(_userContext.TelegramUsername))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        var request = await req.ReadFromJsonAsync<CreateMatchRequest>();
        if (request == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid request body");
            return badRequestResponse;
        }

        if (!await CanManageTournamentAsync(request.TournamentId))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
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
        if (!await CanManageMatchAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
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
        if (!await CanManageMatchAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
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
        if (!await CanManageMatchAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
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

    [Function("AdminReopenMatch")]
    public async Task<HttpResponseData> ReopenMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/reopen-match/{id}")] HttpRequestData req,
        int id)
    {
        if (!await CanManageMatchAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        var match = await _matchStateService.ReopenMatchAsync(id);
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

        if (!await CanManageTournamentAsync(match.TournamentId))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        // Sort voted out slots and convert to CSV
        var sortedSlots = request.VotedOutSlots.OrderBy(s => s).ToList();
        var correctVotedOutCsv = string.Join(",", sortedSlots);

        // Save match result
        await _predictionRepository.SaveMatchResultAsync(id, request.WinningSide, correctVotedOutCsv, request.LastRound);

        // Get correct vote counts
        var correctWinnerVotes = await _predictionRepository.GetCorrectWinnerVotesAsync(id, request.WinningSide);
        var correctVotedOutVotes = await _predictionRepository.GetCorrectVotedOutVotesAsync(id, correctVotedOutCsv);
        var correctLastRoundVotes = await _predictionRepository.GetCorrectLastRoundVotesAsync(id, request.LastRound);

        // Calculate and save scores
        await _scoringService.CalculateAndSaveScoresAsync(id, match.TournamentId, correctWinnerVotes, correctVotedOutVotes, correctLastRoundVotes);

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

    [Function("AdminSetFirstVoted")]
    public async Task<HttpResponseData> SetFirstVotedHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/set-first-voted/{id}")] HttpRequestData req,
        int id)
    {
        var request = await req.ReadFromJsonAsync<SetFirstVotedRequest>();
        if (request == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid request body");
            return badRequestResponse;
        }

        var match = await _matchRepository.GetByIdAsync(id);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match {id} not found");
            return notFoundResponse;
        }

        if (!await CanManageTournamentAsync(match.TournamentId))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        // Sort voted out slots and convert to CSV
        var sortedSlots = request.VotedOutSlots.OrderBy(s => s).ToList();
        var correctVotedOutCsv = string.Join(",", sortedSlots);

        // Save only voted out slots (no winning side yet)
        await _predictionRepository.SaveVotedOutSlotsAsync(id, correctVotedOutCsv);

        // Transition state to FirstVoted
        var updatedMatch = await _matchStateService.SetFirstVotedAsync(id);

        // Publish state to blob
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = updatedMatch.Id,
            GameNumber = updatedMatch.GameNumber,
            TableNumber = updatedMatch.TableNumber,
            State = updatedMatch.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminUndoFirstVoted")]
    public async Task<HttpResponseData> UndoFirstVotedHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/undo-first-voted/{id}")] HttpRequestData req,
        int id)
    {
        var match = await _matchRepository.GetByIdAsync(id);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match {id} not found");
            return notFoundResponse;
        }

        if (!await CanManageTournamentAsync(match.TournamentId))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        // Delete saved voted out slots
        await _predictionRepository.DeleteMatchResultByMatchIdAsync(id);

        // Transition state back to Locked
        var updatedMatch = await _matchStateService.UndoFirstVotedAsync(id);

        // Republish blob state
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = updatedMatch.Id,
            GameNumber = updatedMatch.GameNumber,
            TableNumber = updatedMatch.TableNumber,
            State = updatedMatch.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminUnresolveMatch")]
    public async Task<HttpResponseData> UnresolveMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/unresolve-match/{id}")] HttpRequestData req,
        int id)
    {
        var match = await _matchRepository.GetByIdAsync(id);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match {id} not found");
            return notFoundResponse;
        }

        if (!await CanManageTournamentAsync(match.TournamentId))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        // Rollback scores, match result, and recalculate leaderboard
        await _scoringService.RollbackScoresAsync(id, match.TournamentId);

        // Transition state back to Locked
        var unresolved = await _matchStateService.UnresolveMatchAsync(id);

        // Republish blob state (now shows Locked without match result)
        await _statePublishService.PublishMatchStateAsync(id, forcePublish: true);

        var matchDto = new MatchDto
        {
            Id = unresolved.Id,
            GameNumber = unresolved.GameNumber,
            TableNumber = unresolved.TableNumber,
            State = unresolved.State
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(matchDto);
        return response;
    }

    [Function("AdminDeleteMatch")]
    public async Task<HttpResponseData> DeleteMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/matches/{id}")] HttpRequestData req,
        int id)
    {
        var match = await _matchRepository.GetByIdAsync(id);
        if (match == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Match {id} not found");
            return notFoundResponse;
        }

        if (!await CanManageTournamentAsync(match.TournamentId))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        // Delete in correct order respecting FK constraints:
        // 1. PredictionScore (FK → Prediction)
        await _predictionRepository.DeleteScoresByMatchIdAsync(id);
        // 2. MatchResult (FK → Match)
        await _predictionRepository.DeleteMatchResultByMatchIdAsync(id);
        // 3. Prediction (FK → Match)
        await _predictionRepository.DeleteByMatchIdAsync(id);
        // 4. Match row
        await _matchRepository.DeleteAsync(id);
        // 5. Blob state file
        await _blobWriter.DeleteStateAsync(id);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("AdminPublishState")]
    public async Task<HttpResponseData> PublishStateHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/publish-match-state/{id}")] HttpRequestData req,
        int id)
    {
        if (!await CanManageMatchAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
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
        if (!await CanManageTournamentAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        var matches = await _matchRepository.GetByTournamentAndStateAsync(
            id,
            MatchState.Upcoming,
            MatchState.Open,
            MatchState.Locked,
            MatchState.FirstVoted,
            MatchState.Resolved,
            MatchState.Canceled);

        var matchesList = matches.ToList();

        var stats = new
        {
            TotalMatches = matchesList.Count,
            UpcomingMatches = matchesList.Count(m => m.State == MatchState.Upcoming),
            OpenMatches = matchesList.Count(m => m.State == MatchState.Open),
            LockedMatches = matchesList.Count(m => m.State == MatchState.Locked),
            FirstVotedMatches = matchesList.Count(m => m.State == MatchState.FirstVoted),
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
            stats.FirstVotedMatches,
            stats.ResolvedMatches,
            stats.CanceledMatches,
            TotalPredictions = totalPredictions
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(finalStats);
        return response;
    }

    private async Task<bool> CanManageTournamentAsync(int tournamentId)
    {
        if (_userContext.IsAdmin)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(_userContext.TelegramUsername))
        {
            return false;
        }

        return await _tournamentOperatorRepository.IsOperatorAsync(tournamentId, _userContext.TelegramUsername);
    }

    private async Task<bool> CanManageMatchAsync(int matchId)
    {
        if (_userContext.IsAdmin)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(_userContext.TelegramUsername))
        {
            return false;
        }

        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            return false;
        }

        return await _tournamentOperatorRepository.IsOperatorAsync(match.TournamentId, _userContext.TelegramUsername);
    }
    private static async Task<HttpResponseData> CreateForbiddenResponseAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Forbidden);
        await response.WriteStringAsync(message);
        return response;
    }
}
