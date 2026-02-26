namespace MafiaPickem.Api.Models.Responses;

public class LeaderboardResponse
{
    public List<LeaderboardEntryDto> Entries { get; set; } = new();
    public LeaderboardEntryDto? CurrentUser { get; set; }
}
