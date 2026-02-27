using FluentAssertions;
using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Tests.Models;

public class MatchStateTests
{
    [Fact]
    public void MatchState_Upcoming_ShouldBe0()
    {
        // Arrange & Act
        var value = (byte)MatchState.Upcoming;

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void MatchState_Open_ShouldBe1()
    {
        // Arrange & Act
        var value = (byte)MatchState.Open;

        // Assert
        value.Should().Be(1);
    }

    [Fact]
    public void MatchState_Locked_ShouldBe2()
    {
        // Arrange & Act
        var value = (byte)MatchState.Locked;

        // Assert
        value.Should().Be(2);
    }

    [Fact]
    public void MatchState_Resolved_ShouldBe3()
    {
        // Arrange & Act
        var value = (byte)MatchState.Resolved;

        // Assert
        value.Should().Be(3);
    }

    [Fact]
    public void MatchState_Canceled_ShouldBe4()
    {
        // Arrange & Act
        var value = (byte)MatchState.Canceled;

        // Assert
        value.Should().Be(4);
    }
}
