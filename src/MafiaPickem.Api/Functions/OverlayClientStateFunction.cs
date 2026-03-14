using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.State;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class OverlayClientStateFunction
{
    private readonly ITournamentOperatorRepository _tournamentOperatorRepository;
    private readonly ITournamentOverlayClientStateStore _overlayClientStateStore;
    private readonly IUserContext _userContext;

    public OverlayClientStateFunction(
        ITournamentOperatorRepository tournamentOperatorRepository,
        ITournamentOverlayClientStateStore overlayClientStateStore,
        IUserContext userContext)
    {
        _tournamentOperatorRepository = tournamentOperatorRepository;
        _overlayClientStateStore = overlayClientStateStore;
        _userContext = userContext;
    }

    [Function("ManageTournamentOverlayClientState")]
    public async Task<HttpResponseData> UpdateTournamentOverlayClientStateHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/tournaments/{id}/overlay-client-state")] HttpRequestData req,
        int id)
    {
        if (!await CanManageTournamentAsync(id))
        {
            return await CreateForbiddenResponseAsync(req, "Tournament admin or operator access required");
        }

        var request = await req.ReadFromJsonAsync<UpdateOverlayClientStateRequest>();
        if (request == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid request body");
            return badRequestResponse;
        }

        await _overlayClientStateStore.WriteAsync(new TournamentOverlayClientState
        {
            TournamentId = id,
            ActiveMatchId = request.ActiveMatchId,
            UpdatedAt = DateTime.UtcNow
        });

        return req.CreateResponse(HttpStatusCode.NoContent);
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

    private static async Task<HttpResponseData> CreateForbiddenResponseAsync(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.Forbidden);
        await response.WriteStringAsync(message);
        return response;
    }
}