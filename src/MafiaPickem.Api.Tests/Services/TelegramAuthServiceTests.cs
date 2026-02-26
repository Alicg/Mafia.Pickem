using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using MafiaPickem.Api.Services;
using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Tests.Services;

public class TelegramAuthServiceTests
{
    private const string TestBotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11";
    private readonly ITelegramAuthService _authService;

    public TelegramAuthServiceTests()
    {
        var configValues = new Dictionary<string, string>
        {
            ["TelegramBotToken"] = TestBotToken
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        _authService = new TelegramAuthService(configuration);
    }

    [Fact]
    public void ValidateInitData_WithValidData_ShouldReturnAuthResult()
    {
        // Arrange
        var telegramUser = new
        {
            id = 12345678,
            first_name = "John",
            last_name = "Doe",
            username = "johndoe",
            photo_url = "https://example.com/photo.jpg"
        };

        var initData = GenerateValidInitData(telegramUser);

        // Act
        var result = _authService.ValidateInitData(initData);

        // Assert
        result.Should().NotBeNull();
        result!.TelegramId.Should().Be(12345678);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Username.Should().Be("johndoe");
        result.PhotoUrl.Should().Be("https://example.com/photo.jpg");
    }

    [Fact]
    public void ValidateInitData_WithInvalidHash_ShouldReturnNull()
    {
        // Arrange
        var telegramUser = new
        {
            id = 12345678,
            first_name = "John"
        };

        var initData = GenerateValidInitData(telegramUser);
        // Tamper with the hash
        initData = initData.Replace(initData.Split('&')[0].Split('=')[1].Substring(0, 5), "00000");

        // Act
        var result = _authService.ValidateInitData(initData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateInitData_WithMissingHash_ShouldReturnNull()
    {
        // Arrange
        var userJson = JsonSerializer.Serialize(new { id = 12345678, first_name = "John" });
        var initData = $"auth_date=1234567890&user={Uri.EscapeDataString(userJson)}";

        // Act
        var result = _authService.ValidateInitData(initData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateInitData_WithMinimalUserData_ShouldReturnAuthResult()
    {
        // Arrange
        var telegramUser = new
        {
            id = 987654321,
            first_name = "Jane"
        };

        var initData = GenerateValidInitData(telegramUser);

        // Act
        var result = _authService.ValidateInitData(initData);

        // Assert
        result.Should().NotBeNull();
        result!.TelegramId.Should().Be(987654321);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().BeNull();
        result.Username.Should().BeNull();
        result.PhotoUrl.Should().BeNull();
    }

    [Fact]
    public void ValidateInitData_WithEmptyString_ShouldReturnNull()
    {
        // Act
        var result = _authService.ValidateInitData(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateInitData_WithMalformedData_ShouldReturnNull()
    {
        // Arrange
        var initData = "this-is-not-valid-data";

        // Act
        var result = _authService.ValidateInitData(initData);

        // Assert
        result.Should().BeNull();
    }

    // Helper method to generate valid initData with correct HMAC
    private string GenerateValidInitData(object telegramUser)
    {
        var authDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var userJson = JsonSerializer.Serialize(telegramUser);
        
        var dataCheckString = $"auth_date={authDate}\nuser={userJson}";
        
        // Calculate HMAC according to Telegram's algorithm
        var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(TestBotToken));
        var hash = HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString));
        var hashHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return $"hash={hashHex}&auth_date={authDate}&user={Uri.EscapeDataString(userJson)}";
    }
}
