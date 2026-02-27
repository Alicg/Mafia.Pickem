namespace MafiaPickem.Api.Models.Requests;

public class CreateTournamentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
