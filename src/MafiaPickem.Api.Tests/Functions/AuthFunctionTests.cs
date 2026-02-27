using FluentAssertions;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Functions;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MafiaPickem.Api.Tests.Functions;

public class AuthFunctionTests
{
    private readonly Mock<ITelegramAuthService> _telegramAuthMock;
    private readonly Mock<IPickemUserRepository> _repositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IConfiguration _configuration;
    private readonly AuthFunction _function;

    public AuthFunctionTests()
    {
        _telegramAuthMock = new Mock<ITelegramAuthService>();
        _repositoryMock = new Mock<IPickemUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();

        var configValues = new Dictionary<string, string>
        {
            ["PickemAdminTelegramIds"] = "111,222,333"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        _function = new AuthFunction(
            _telegramAuthMock.Object,
            _repositoryMock.Object,
            _jwtServiceMock.Object,
            _configuration
        );
    }

    [Fact]
    public async Task AuthenticateTelegram_WithValidInitData_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new TelegramAuthRequest { InitData = "valid_init_data" };
        var telegramResult = new TelegramAuthResult
        {
            TelegramId = 12345,
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhotoUrl = "https://example.com/photo.jpg"
        };
        var user = new PickemUser
        {
            Id = 1,
            TelegramId = 12345,
            GameNickname = "_unregistered_12345",
            PhotoUrl = "https://example.com/photo.jpg"
        };
        var token = "jwt_token_here";

        _telegramAuthMock.Setup(x => x.ValidateInitData(request.InitData))
            .Returns(telegramResult);
        _repositoryMock.Setup(x => x.UpsertByTelegramIdAsync(12345, "https://example.com/photo.jpg"))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user, false))
            .Returns(token);

        // Act
        var result = await _function.AuthenticateTelegram(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(1);
        result.User.TelegramId.Should().Be(12345);
        result.User.IsRegistered.Should().BeFalse();
        result.User.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateTelegram_WithAdminUser_ShouldSetIsAdminTrue()
    {
        // Arrange
        var request = new TelegramAuthRequest { InitData = "valid_init_data" };
        var telegramResult = new TelegramAuthResult
        {
            TelegramId = 111, // This is in the admin list
            FirstName = "Admin",
            PhotoUrl = null
        };
        var user = new PickemUser
        {
            Id = 2,
            TelegramId = 111,
            GameNickname = "_unregistered_111"
        };
        var token = "admin_jwt_token";

        _telegramAuthMock.Setup(x => x.ValidateInitData(request.InitData))
            .Returns(telegramResult);
        _repositoryMock.Setup(x => x.UpsertByTelegramIdAsync(111, null))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user, true))
            .Returns(token);

        // Act
        var result = await _function.AuthenticateTelegram(request);

        // Assert
        result.User.IsAdmin.Should().BeTrue();
        _jwtServiceMock.Verify(x => x.GenerateToken(user, true), Times.Once);
    }

    [Fact]
    public async Task AuthenticateTelegram_WithRegisteredUser_ShouldSetIsRegisteredTrue()
    {
        // Arrange
        var request = new TelegramAuthRequest { InitData = "valid_init_data" };
        var telegramResult = new TelegramAuthResult
        {
            TelegramId = 54321,
            FirstName = "Jane"
        };
        var user = new PickemUser
        {
            Id = 3,
            TelegramId = 54321,
            GameNickname = "JaneDoe", // Registered nickname
            PhotoUrl = null
        };
        var token = "jwt_token_registered";

        _telegramAuthMock.Setup(x => x.ValidateInitData(request.InitData))
            .Returns(telegramResult);
        _repositoryMock.Setup(x => x.UpsertByTelegramIdAsync(54321, null))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user, false))
            .Returns(token);

        // Act
        var result = await _function.AuthenticateTelegram(request);

        // Assert
        result.User.IsRegistered.Should().BeTrue();
        result.User.GameNickname.Should().Be("JaneDoe");
    }

    [Fact]
    public async Task AuthenticateTelegram_WithInvalidInitData_ShouldThrowException()
    {
        // Arrange
        var request = new TelegramAuthRequest { InitData = "invalid_init_data" };

        _telegramAuthMock.Setup(x => x.ValidateInitData(request.InitData))
            .Returns((TelegramAuthResult?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _function.AuthenticateTelegram(request));
    }

    [Fact]
    public async Task AuthenticateTelegram_WithEmptyAdminConfig_ShouldWorkWithoutAdmins()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["PickemAdminTelegramIds"] = ""
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        var function = new AuthFunction(
            _telegramAuthMock.Object,
            _repositoryMock.Object,
            _jwtServiceMock.Object,
            config
        );

        var request = new TelegramAuthRequest { InitData = "valid_init_data" };
        var telegramResult = new TelegramAuthResult { TelegramId = 12345, FirstName = "John" };
        var user = new PickemUser
        {
            Id = 1,
            TelegramId = 12345,
            GameNickname = "_unregistered_12345"
        };
        var token = "jwt_token";

        _telegramAuthMock.Setup(x => x.ValidateInitData(request.InitData))
            .Returns(telegramResult);
        _repositoryMock.Setup(x => x.UpsertByTelegramIdAsync(12345, null))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user, false))
            .Returns(token);

        // Act
        var result = await function.AuthenticateTelegram(request);

        // Assert
        result.User.IsAdmin.Should().BeFalse();
    }
}
