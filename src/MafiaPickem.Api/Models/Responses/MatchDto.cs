using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Models.Responses;

public class MatchDto
{
    public int Id { get; set; }
    public int GameNumber { get; set; }
    public int? TableNumber { get; set; }
    public MatchState State { get; set; }
    public PredictionDto? MyPrediction { get; set; }
    public VoteStatsDto? VoteStats { get; set; }
}
