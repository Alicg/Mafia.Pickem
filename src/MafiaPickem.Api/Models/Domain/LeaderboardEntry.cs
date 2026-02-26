namespace MafiaPickem.Api.Models.Domain;

public class LeaderboardEntry
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int UserId { get; set; }
    public decimal TotalPoints { get; set; }
    public int CorrectPredictions { get; set; }
    public int TotalPredictions { get; set; }
    public int? Rank { get; set; }
    public DateTime DateUpdated { get; set; }
}
