namespace MafiaPickem.Api.Models.Domain;

public class Prediction
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int UserId { get; set; }
    public byte PredictedWinner { get; set; }
    public byte PredictedVotedOut { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
