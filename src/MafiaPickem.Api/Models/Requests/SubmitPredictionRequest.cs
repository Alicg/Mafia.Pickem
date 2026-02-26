namespace MafiaPickem.Api.Models.Requests;

public class SubmitPredictionRequest
{
    public byte PredictedWinner { get; set; }
    public byte PredictedVotedOut { get; set; }
}
