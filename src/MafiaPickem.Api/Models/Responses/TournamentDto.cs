using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Models.Responses;

public class TournamentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Teams { get; set; } = new();
    public string? SelectedTeamName { get; set; }
    public bool CanManage { get; set; }
    public MatchDto? CurrentMatch { get; set; }
}
