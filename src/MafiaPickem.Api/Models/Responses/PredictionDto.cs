namespace MafiaPickem.Api.Models.Responses;

public class PredictionDto
{
    public byte PredictedWinner { get; set; }
    public byte PredictedVotedOut { get; set; }
    public decimal? WinnerPoints { get; set; }
    public decimal? VotedOutPoints { get; set; }
    public decimal? TotalPoints { get; set; }
}
