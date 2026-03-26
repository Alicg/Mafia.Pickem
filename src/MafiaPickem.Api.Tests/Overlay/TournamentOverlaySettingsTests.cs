using FluentAssertions;
using MafiaPickem.Api.Overlay;

namespace MafiaPickem.Api.Tests.Overlay;

public class TournamentOverlaySettingsTests
{
    [Fact]
    public void Deserialize_ShouldDefaultBlockVisibilityToTrue_WhenOldJsonDoesNotContainFlags()
    {
        var settings = TournamentOverlaySettingsSerializer.Deserialize("""
            {
              "hideBlocksByPhase": false,
              "summaryBlock": {
                "panel": "right"
              },
              "firstVoteBlock": {
                "panel": "left"
              }
            }
            """);

        settings.HideBlocksByPhase.Should().BeFalse();
        settings.SummaryBlock.Panel.Should().Be(OverlayPanelSide.Right);
        settings.SummaryBlock.IsVisible.Should().BeTrue();
        settings.SummaryBlock.DynamicDisplay.Enabled.Should().BeFalse();
        settings.SummaryBlock.DynamicDisplay.IntervalSeconds.Should().Be(30);
        settings.SummaryBlock.DynamicDisplay.VisibleDurationSeconds.Should().Be(8);
        settings.FirstVoteBlock.Panel.Should().Be(OverlayPanelSide.Left);
        settings.FirstVoteBlock.IsVisible.Should().BeTrue();
        settings.LastRoundBlock.IsVisible.Should().BeTrue();
        settings.FooterBlock.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_ShouldNormalizeDynamicDisplayRange()
    {
        var settings = TournamentOverlaySettingsSerializer.Deserialize("""
            {
              "summaryBlock": {
                "panel": "left",
                "dynamicDisplay": {
                  "enabled": true,
                  "intervalSeconds": 3,
                  "visibleDurationSeconds": 9
                }
              }
            }
            """);

        settings.SummaryBlock.DynamicDisplay.Enabled.Should().BeTrue();
        settings.SummaryBlock.DynamicDisplay.IntervalSeconds.Should().Be(3);
        settings.SummaryBlock.DynamicDisplay.VisibleDurationSeconds.Should().Be(3);
    }
}