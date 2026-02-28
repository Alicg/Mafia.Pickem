using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class MatchFunctions
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IPredictionService _predictionService;
    private readonly IStatePublishService _statePublishService;
    private readonly IUserContext _userContext;
    private readonly ILogger<MatchFunctions> _logger;

    public MatchFunctions(
        IMatchRepository matchRepository,
        IPredictionRepository predictionRepository,
        IPredictionService predictionService,
        IStatePublishService statePublishService,
        IUserContext userContext,
        ILogger<MatchFunctions>? logger = null)
    {
        _matchRepository = matchRepository;
        _predictionRepository = predictionRepository;
        _predictionService = predictionService;
        _statePublishService = statePublishService;
        _userContext = userContext;
        _logger = logger ?? null!;
    }

    [Function("GetMatch")]
    public async Task<HttpResponseData> GetMatchHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "matches/{id}")] HttpRequestData req,
        int id)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(id);
            if (match == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Match with ID {id} not found");
                return notFoundResponse;
            }

            PredictionDto? myPrediction = null;

            // Load user's prediction if user is registered
            if (_userContext.IsRegistered)
            {
                var prediction = await _predictionRepository.GetByMatchAndUserAsync(id, _userContext.UserId);
                if (prediction != null)
                {
                    myPrediction = new PredictionDto
                    {
                        PredictedWinner = prediction.PredictedWinner,
                        PredictedVotedOut = prediction.PredictedVotedOut
                    };

                    // If match is resolved, include score
                    if (match.State == MatchState.Resolved)
                    {
                        var score = await _predictionRepository.GetScoreByPredictionIdAsync(prediction.Id);
                        if (score != null)
                        {
                            myPrediction.WinnerPoints = score.WinnerPoints;
                            myPrediction.VotedOutPoints = score.VotedOutPoints;
                            myPrediction.TotalPoints = score.TotalPoints;
                        }
                    }
                }
            }

            var matchDto = new MatchDto
            {
                Id = match.Id,
                GameNumber = match.GameNumber,
                TableNumber = match.TableNumber,
                State = match.State,
                MyPrediction = myPrediction
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(matchDto);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting match {MatchId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    [Function("SubmitPrediction")]
    public async Task<HttpResponseData> SubmitPredictionHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "matches/{id}/predict")] HttpRequestData req,
        int id)
    {
        try
        {
            // Validate user is registered
            if (!_userContext.IsRegistered)
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("User must be registered to submit predictions");
                return unauthorizedResponse;
            }

            var request = await req.ReadFromJsonAsync<SubmitPredictionRequest>();
            if (request == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            await _predictionService.SubmitPredictionAsync(
                id,
                _userContext.UserId,
                request.PredictedWinner,
                request.PredictedVotedOut);

            // Publish state update to blob (with throttling for Open state)
            await _statePublishService.PublishMatchStateAsync(id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        catch (InvalidOperationException ex)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync(ex.Message);
            return badRequestResponse;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error submitting prediction for match {MatchId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }
}
