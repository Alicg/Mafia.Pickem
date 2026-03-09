namespace MafiaPickem.Api.Overlay;

public interface IObsOverlayService
{
    Task<ObsOverlayPayload> GetOverlayPayloadAsync(int tournamentId);
}
