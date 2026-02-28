namespace MafiaPickem.Api.Models.Responses;

public class LeaderboardEntryDto
{
    public int UserId { get; set; }
    public int Rank { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public decimal TotalPoints { get; set; }
    public int CorrectPredictions { get; set; }
    public int TotalPredictions { get; set; }
}
