namespace MafiaPickem.Api.Models.Domain;

public class PredictionScore
{
    public int Id { get; set; }
    public int PredictionId { get; set; }
    public decimal WinnerPoints { get; set; }
    public decimal VotedOutPoints { get; set; }
    public decimal TotalPoints { get; set; }
    public int TotalVotes { get; set; }
    public int CorrectWinnerVotes { get; set; }
    public int CorrectVotedOutVotes { get; set; }
    public DateTime DateCalculated { get; set; }
}
