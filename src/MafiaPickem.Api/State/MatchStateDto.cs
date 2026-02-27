namespace MafiaPickem.Api.State;

public class BlobMatchState
{
    public int MatchId { get; set; }
    public int TournamentId { get; set; }
    public long Version { get; set; }
    public string State { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public int TableSize { get; set; } = 10;
    public int TotalPredictions { get; set; }
    public WinnerVotesDto? WinnerVotes { get; set; }
    public List<SlotVoteEntry>? VotedOutVotes { get; set; }
}

public class WinnerVotesDto
{
    public VoteEntry Town { get; set; } = new();
    public VoteEntry Mafia { get; set; } = new();
}

public class VoteEntry
{
    public int Count { get; set; }
    public decimal Percent { get; set; }
}

public class SlotVoteEntry
{
    public int Slot { get; set; }
    public int Count { get; set; }
    public decimal Percent { get; set; }
}
