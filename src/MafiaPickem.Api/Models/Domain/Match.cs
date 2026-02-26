using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Models.Domain;

public class Match
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public string? ExternalMatchRef { get; set; }
    public int GameNumber { get; set; }
    public int? TableNumber { get; set; }
    public MatchState State { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateOpened { get; set; }
    public DateTime? DateLocked { get; set; }
    public DateTime? DateResolved { get; set; }
}
