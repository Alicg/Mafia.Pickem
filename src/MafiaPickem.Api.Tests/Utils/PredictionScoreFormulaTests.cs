using FluentAssertions;
using MafiaPickem.Api.Utils;

namespace MafiaPickem.Api.Tests.Utils;

public class PredictionScoreFormulaTests
{
    [Fact]
    public void CalculatePoints_WithNoVotes_ReturnsZero()
    {
        PredictionScoreFormula.CalculatePoints(0, 0).Should().Be(0m);
        PredictionScoreFormula.CalculatePoints(100, 0).Should().Be(0m);
    }

    [Fact]
    public void CalculatePoints_WhenEveryoneIsCorrect_ReturnsBasePoints()
    {
        PredictionScoreFormula.CalculatePoints(100, 100).Should().Be(15m);
    }

    [Theory]
    [InlineData(50, 20.2500)]
    [InlineData(20, 27.1901)]
    [InlineData(10, 32.4401)]
    [InlineData(2, 44.6302)]
    [InlineData(1, 45.0000)]
    public void CalculatePoints_WithRarerOutcomes_IncreasesUpToCap(int correctVotes, decimal expectedPoints)
    {
        PredictionScoreFormula.CalculatePoints(100, correctVotes).Should().Be(expectedPoints);
    }

    [Fact]
    public void Formula_UsesSharedParameters_ForAllBetTypes()
    {
        PredictionScoreFormula.BasePoints.Should().Be(15m);
        PredictionScoreFormula.RarityAlpha.Should().Be(0.35m);
        PredictionScoreFormula.RarityCap.Should().Be(3.0m);
    }
}