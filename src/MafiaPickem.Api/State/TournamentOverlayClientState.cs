namespace MafiaPickem.Api.State;

public class TournamentOverlayClientState
{
    public int TournamentId { get; set; }
    public int? ActiveMatchId { get; set; }
    public DateTime UpdatedAt { get; set; }
}