using FluentAssertions;
using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Tests.Models;

public class WinningSideTests
{
    [Fact]
    public void WinningSide_Town_ShouldBe0()
    {
        // Arrange & Act
        var value = (byte)WinningSide.Town;

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void WinningSide_Mafia_ShouldBe1()
    {
        // Arrange & Act
        var value = (byte)WinningSide.Mafia;

        // Assert
        value.Should().Be(1);
    }
}
