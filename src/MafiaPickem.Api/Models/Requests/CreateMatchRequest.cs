namespace MafiaPickem.Api.Models.Requests;

public class CreateMatchRequest
{
    public int TournamentId { get; set; }
    public int GameNumber { get; set; }
    public int? TableNumber { get; set; }
    public string? ExternalMatchRef { get; set; }
}
