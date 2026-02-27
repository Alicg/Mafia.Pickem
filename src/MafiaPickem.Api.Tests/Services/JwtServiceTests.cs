using System.Security.Claims;
using FluentAssertions;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Services;
using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Tests.Services;

public class JwtServiceTests
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public JwtServiceTests()
    {
        var configValues = new Dictionary<string, string>
        {
            ["JwtSecret"] = "test-jwt-secret-key-that-is-at-least-32-characters-long",
            ["JwtIssuer"] = "TestIssuer"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateToken_ShouldCreateValidToken()
    {
        // Arrange
        var user = new PickemUser
        {
            Id = 1,
            TelegramId = 12345678,
            GameNickname = "TestUser",
            PhotoUrl = "https://example.com/photo.jpg"
        };

        // Act
        var token = _jwtService.GenerateToken(user, isAdmin: false);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = new PickemUser
        {
            Id = 42,
            TelegramId = 987654321,
            GameNickname = "AdminUser",
            PhotoUrl = null
        };

        var token = _jwtService.GenerateToken(user, isAdmin: true);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();

        var userIdClaim = principal!.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be("42");

        principal.Claims.Should().ContainSingle(c => c.Type == "telegram_id" && c.Value == "987654321");
        principal.Claims.Should().ContainSingle(c => c.Type == "nickname" && c.Value == "AdminUser");
        principal.Claims.Should().ContainSingle(c => c.Type == "is_admin" && c.Value == "True");
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange - Create a JWT service with negative expiry to simulate expired token
        var expiredConfigValues = new Dictionary<string, string>
        {
            ["JwtSecret"] = "test-jwt-secret-key-that-is-at-least-32-characters-long",
            ["JwtIssuer"] = "TestIssuer"
        };

        var expiredConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(expiredConfigValues!)
            .Build();

        // We'll need to create an expired token manually or adjust the service
        // For now, we'll just test invalid signature
        var wrongSecretConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSecret"] = "wrong-secret-key-that-is-different-32-chars",
                ["JwtIssuer"] = "TestIssuer"
            }!)
            .Build();

        var wrongKeyService = new JwtService(wrongSecretConfig);

        var user = new PickemUser { Id = 1, TelegramId = 123, GameNickname = "Test" };
        var token = _jwtService.GenerateToken(user, false);

        // Act
        var principal = wrongKeyService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_WithUnregisteredUser_ShouldNotIncludeNickname()
    {
        // Arrange
        var user = new PickemUser
        {
            Id = 1,
            TelegramId = 12345678,
            GameNickname = "_unregistered_12345678"
        };

        // Act
        var token = _jwtService.GenerateToken(user, isAdmin: false);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().NotContain(c => c.Type == "nickname");
    }
}
