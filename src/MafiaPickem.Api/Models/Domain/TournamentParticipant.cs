namespace MafiaPickem.Api.Models.Domain;

public class TournamentParticipant
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int UserId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}