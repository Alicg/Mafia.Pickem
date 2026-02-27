using FluentAssertions;
using MafiaPickem.Api.Bot;
using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Tests.Bot;

public class TelegramWebhookValidatorTests
{
    [Fact]
    public void Validate_WithCorrectSecret_ShouldReturnTrue()
    {
        // Arrange
        var expectedSecret = "test-secret-token-12345";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TelegramWebhookSecretToken"] = expectedSecret
            }!)
            .Build();

        var validator = new TelegramWebhookValidator(config);

        // Act
        var result = validator.ValidateSecretToken(expectedSecret);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithIncorrectSecret_ShouldReturnFalse()
    {
        // Arrange
        var expectedSecret = "test-secret-token-12345";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TelegramWebhookSecretToken"] = expectedSecret
            }!)
            .Build();

        var validator = new TelegramWebhookValidator(config);

        // Act
        var result = validator.ValidateSecretToken("wrong-secret");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNullHeader_ShouldReturnFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TelegramWebhookSecretToken"] = "test-secret-token"
            }!)
            .Build();

        var validator = new TelegramWebhookValidator(config);

        // Act
        var result = validator.ValidateSecretToken(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptyHeader_ShouldReturnFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TelegramWebhookSecretToken"] = "test-secret-token"
            }!)
            .Build();

        var validator = new TelegramWebhookValidator(config);

        // Act
        var result = validator.ValidateSecretToken("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenSecretNotConfigured_ShouldReturnFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        var validator = new TelegramWebhookValidator(config);

        // Act
        var result = validator.ValidateSecretToken("any-token");

        // Assert
        result.Should().BeFalse();
    }
}
