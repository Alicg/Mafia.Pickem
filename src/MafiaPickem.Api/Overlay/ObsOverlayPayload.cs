namespace MafiaPickem.Api.Overlay;

public class ObsOverlayPayload
{
    public string Status { get; set; } = "no-match";
    public string Message { get; set; } = string.Empty;
    public int TournamentId { get; set; }
    public int? MatchId { get; set; }
    public int? GameNumber { get; set; }
    public int? TableNumber { get; set; }
    public string MatchState { get; set; } = string.Empty;
    public string MatchStateLabel { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public int TotalPredictions { get; set; }
    public byte? WinningSide { get; set; }
    public OverlaySideStat RedSide { get; set; } = new();
    public OverlaySideStat BlackSide { get; set; } = new();
    public List<OverlaySeatVote> SeatVotes { get; set; } = new();
    public List<int> ResolvedSlots { get; set; } = new();
}

public class OverlaySideStat
{
    public int Count { get; set; }
    public decimal Percent { get; set; }
}

public class OverlaySeatVote
{
    public int Slot { get; set; }
    public int Count { get; set; }
    public decimal Percent { get; set; }
    public bool IsResolved { get; set; }
}
