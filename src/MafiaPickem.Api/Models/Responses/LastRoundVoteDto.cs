namespace MafiaPickem.Api.Models.Responses;

public class LastRoundVoteDto
{
    public byte LastRound { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
