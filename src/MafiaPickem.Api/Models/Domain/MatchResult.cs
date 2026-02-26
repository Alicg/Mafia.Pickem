using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Models.Domain;

public class MatchResult
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public WinningSide WinningSide { get; set; }
    public string CorrectVotedOutCsv { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}
