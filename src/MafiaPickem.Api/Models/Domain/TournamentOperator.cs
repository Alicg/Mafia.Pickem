namespace MafiaPickem.Api.Models.Domain;

public class TournamentOperator
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int UserId { get; set; }
    public DateTime DateCreated { get; set; }
}