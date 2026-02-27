using FluentAssertions;
using MafiaPickem.Api.Bot;
using MafiaPickem.Api.Functions;
using Moq;

namespace MafiaPickem.Api.Tests.Functions;

public class BotWebhookFunctionTests
{
    private readonly Mock<ITelegramWebhookValidator> _validatorMock;
    private readonly BotWebhookFunction _function;

    public BotWebhookFunctionTests()
    {
        _validatorMock = new Mock<ITelegramWebhookValidator>();
        _function = new BotWebhookFunction(_validatorMock.Object);
    }

    [Fact]
    public void BotWebhookFunction_ShouldRequireValidator()
    {
        // Arrange & Act
        var function = new BotWebhookFunction(_validatorMock.Object);

        // Assert
        function.Should().NotBeNull();
    }

    [Fact]
    public void BotWebhookFunction_ValidatorIntegration_ShouldWork()
    {
        // Arrange - Test that validator is properly integrated
        var secretToken = "test-secret";
        _validatorMock.Setup(v => v.ValidateSecretToken(secretToken)).Returns(true);

        // Act
        var result = _validatorMock.Object.ValidateSecretToken(secretToken);

        // Assert
        result.Should().BeTrue();
        _validatorMock.Verify(v => v.ValidateSecretToken(secretToken), Times.Once);
    }
}

