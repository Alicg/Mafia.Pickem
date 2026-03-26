namespace MafiaPickem.Api.Models.Requests;

public class CreateTournamentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Teams { get; set; } = new();
    public List<string> OperatorUsernames { get; set; } = new();
    public bool? VisibleOnHomePage { get; set; }
}
