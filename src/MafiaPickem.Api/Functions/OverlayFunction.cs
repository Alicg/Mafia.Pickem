using MafiaPickem.Api.Overlay;
using MafiaPickem.Api.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class OverlayFunction
{
    private readonly IObsOverlayService _overlayService;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<OverlayFunction> _logger;

    public OverlayFunction(
        IObsOverlayService overlayService,
        ITournamentRepository tournamentRepository,
        ILogger<OverlayFunction>? logger = null)
    {
        _overlayService = overlayService;
        _tournamentRepository = tournamentRepository;
        _logger = logger ?? null!;
    }

    [Function("GetObsOverlay")]
    public async Task<HttpResponseData> GetObsOverlayHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "overlay/tournaments/{tournamentId}")] HttpRequestData req,
        int tournamentId)
    {
        try
        {
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            var overlaySettings = TournamentOverlaySettingsSerializer.Deserialize(tournament?.OverlaySettingsJson);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate");
            var html = overlaySettings.OverlayType == ObsOverlayType.ViewerSympathy
                ? ObsViewerSympathyHtmlRenderer.Render(tournamentId)
                : ObsOverlayHtmlRenderer.Render(tournamentId);
            await response.WriteStringAsync(html);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rendering OBS overlay for tournament {TournamentId}", tournamentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    [Function("GetObsOverlayData")]
    public async Task<HttpResponseData> GetObsOverlayDataHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "overlay/tournaments/{tournamentId}/data")] HttpRequestData req,
        int tournamentId)
    {
        try
        {
            var payload = await _overlayService.GetOverlayPayloadAsync(tournamentId);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate");
            await response.WriteAsJsonAsync(payload);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting OBS overlay data for tournament {TournamentId}", tournamentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }
}
