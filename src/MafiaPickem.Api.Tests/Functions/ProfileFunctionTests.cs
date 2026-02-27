using FluentAssertions;
using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Functions;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Moq;

namespace MafiaPickem.Api.Tests.Functions;

public class ProfileFunctionTests
{
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<INicknameService> _nicknameServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPickemUserRepository> _repositoryMock;
    private readonly ProfileFunction _function;

    public ProfileFunctionTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _nicknameServiceMock = new Mock<INicknameService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _repositoryMock = new Mock<IPickemUserRepository>();

        _function = new ProfileFunction(
            _userContextMock.Object,
            _nicknameServiceMock.Object,
            _jwtServiceMock.Object,
            _repositoryMock.Object
        );
    }

    [Fact]
    public void GetProfile_ShouldReturnCurrentUserProfile()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserId).Returns(1);
        _userContextMock.Setup(x => x.TelegramId).Returns(12345);
        _userContextMock.Setup(x => x.GameNickname).Returns("TestUser");
        _userContextMock.Setup(x => x.IsRegistered).Returns(true);
        _userContextMock.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = _function.GetProfile();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.TelegramId.Should().Be(12345);
        result.GameNickname.Should().Be("TestUser");
        result.IsRegistered.Should().BeTrue();
        result.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public void GetProfile_WithUnregisteredUser_ShouldReturnUnregistered()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserId).Returns(2);
        _userContextMock.Setup(x => x.TelegramId).Returns(54321);
        _userContextMock.Setup(x => x.GameNickname).Returns("_unregistered_54321");
        _userContextMock.Setup(x => x.IsRegistered).Returns(false);
        _userContextMock.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = _function.GetProfile();

        // Assert
        result.IsRegistered.Should().BeFalse();
        result.GameNickname.Should().Be("_unregistered_54321");
    }

    [Fact]
    public void GetProfile_WithAdminUser_ShouldReturnAdmin()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserId).Returns(3);
        _userContextMock.Setup(x => x.TelegramId).Returns(111);
        _userContextMock.Setup(x => x.GameNickname).Returns("AdminUser");
        _userContextMock.Setup(x => x.IsRegistered).Returns(true);
        _userContextMock.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = _function.GetProfile();

        // Assert
        result.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNickname_WithValidNickname_ShouldReturnAuthResponseWithNewToken()
    {
        // Arrange
        var request = new UpdateNicknameRequest { GameNickname = "NewNickname" };
        var userId = 1;
        var updatedUser = new PickemUser
        {
            Id = userId,
            TelegramId = 12345,
            GameNickname = "NewNickname",
            PhotoUrl = "https://example.com/photo.jpg"
        };
        var newToken = "new_jwt_token";

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.IsAdmin).Returns(false);
        _nicknameServiceMock.Setup(x => x.ValidateAndSaveNicknameAsync(userId, "NewNickname"))
            .ReturnsAsync(NicknameValidationResult.Success());
        _repositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(updatedUser);
        _jwtServiceMock.Setup(x => x.GenerateToken(updatedUser, false))
            .Returns(newToken);

        // Act
        var result = await _function.UpdateNickname(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(newToken);
        result.User.GameNickname.Should().Be("NewNickname");
        result.User.IsRegistered.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNickname_WithInvalidNickname_ShouldThrowException()
    {
        // Arrange
        var request = new UpdateNicknameRequest { GameNickname = "ab" }; // Too short
        var userId = 1;

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _nicknameServiceMock.Setup(x => x.ValidateAndSaveNicknameAsync(userId, "ab"))
            .ReturnsAsync(NicknameValidationResult.Fail("Nickname must be at least 3 characters long"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _function.UpdateNickname(request));
        exception.Message.Should().Be("Nickname must be at least 3 characters long");
    }

    [Fact]
    public async Task UpdateNickname_WithTakenNickname_ShouldThrowException()
    {
        // Arrange
        var request = new UpdateNicknameRequest { GameNickname = "TakenNick" };
        var userId = 1;

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _nicknameServiceMock.Setup(x => x.ValidateAndSaveNicknameAsync(userId, "TakenNick"))
            .ReturnsAsync(NicknameValidationResult.Fail("Nickname is already taken"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _function.UpdateNickname(request));
        exception.Message.Should().Be("Nickname is already taken");
    }

    [Fact]
    public async Task UpdateNickname_WithAdminUser_ShouldGenerateTokenWithAdminFlag()
    {
        // Arrange
        var request = new UpdateNicknameRequest { GameNickname = "AdminNick" };
        var userId = 1;
        var updatedUser = new PickemUser
        {
            Id = userId,
            TelegramId = 111,
            GameNickname = "AdminNick"
        };
        var newToken = "admin_jwt_token";

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userContextMock.Setup(x => x.IsAdmin).Returns(true);
        _nicknameServiceMock.Setup(x => x.ValidateAndSaveNicknameAsync(userId, "AdminNick"))
            .ReturnsAsync(NicknameValidationResult.Success());
        _repositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(updatedUser);
        _jwtServiceMock.Setup(x => x.GenerateToken(updatedUser, true))
            .Returns(newToken);

        // Act
        var result = await _function.UpdateNickname(request);

        // Assert
        result.Token.Should().Be(newToken);
        _jwtServiceMock.Verify(x => x.GenerateToken(updatedUser, true), Times.Once);
    }
}
