namespace MafiaPickem.Api.Models.Domain;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? TeamsJson { get; set; }
    public bool Active { get; set; }
    public bool VisibleOnHomePage { get; set; }
    public bool ShowTeamSelection { get; set; } = true;
    public DateTime DateCreated { get; set; }
}
