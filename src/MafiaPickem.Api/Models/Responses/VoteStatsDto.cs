namespace MafiaPickem.Api.Models.Responses;

public class VoteStatsDto
{
    public int TotalVotes { get; set; }
    public decimal TownPercentage { get; set; }
    public decimal MafiaPercentage { get; set; }
    public List<SlotVoteDto> SlotVotes { get; set; } = new();
}
