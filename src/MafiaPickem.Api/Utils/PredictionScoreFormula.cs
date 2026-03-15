namespace MafiaPickem.Api.Utils;

public static class PredictionScoreFormula
{
    public const decimal BasePoints = 15m;
    public const decimal RarityAlpha = 0.35m;
    public const decimal RarityCap = 3.0m;

    public static decimal CalculatePoints(int totalVotes, int correctVotes)
    {
        if (totalVotes <= 0 || correctVotes <= 0)
        {
            return 0m;
        }

        var multiplier = 1m + RarityAlpha * (decimal)Math.Log((double)totalVotes / correctVotes, 2d);
        return decimal.Round(BasePoints * Math.Min(RarityCap, multiplier), 4, MidpointRounding.AwayFromZero);
    }
}