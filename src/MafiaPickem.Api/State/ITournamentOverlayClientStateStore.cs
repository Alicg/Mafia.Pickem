namespace MafiaPickem.Api.State;

public interface ITournamentOverlayClientStateStore
{
    Task<TournamentOverlayClientState?> ReadAsync(int tournamentId);
    Task WriteAsync(TournamentOverlayClientState state);
}