using FluentAssertions;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Services;
using Moq;

namespace MafiaPickem.Api.Tests.Services;

public class NicknameServiceTests
{
    private readonly Mock<IPickemUserRepository> _mockRepository;
    private readonly INicknameService _nicknameService;

    public NicknameServiceTests()
    {
        _mockRepository = new Mock<IPickemUserRepository>();
        _nicknameService = new NicknameService(_mockRepository.Object);
    }

    [Fact]
    public async Task ValidateAndSaveNickname_WithValidNickname_ShouldSucceed()
    {
        // Arrange
        var userId = 1;
        var nickname = "ValidNickname";
        _mockRepository.Setup(r => r.IsNicknameAvailableAsync(nickname, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateNicknameAsync(userId, nickname), Times.Once);
    }

    [Theory]
    [InlineData("a")] // too short
    [InlineData("x")] // too short
    public async Task ValidateAndSaveNickname_WithTooShortNickname_ShouldFail(string nickname)
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at least 2 characters");
        _mockRepository.Verify(r => r.UpdateNicknameAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAndSaveNickname_WithTooLongNickname_ShouldFail()
    {
        // Arrange
        var userId = 1;
        var nickname = new string('a', 31); // 31 characters

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("maximum 30 characters");
        _mockRepository.Verify(r => r.UpdateNicknameAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("Nick@me")]
    [InlineData("User#123")]
    [InlineData("Player$")]
    [InlineData("Test%Name")]
    [InlineData("Name&")]
    public async Task ValidateAndSaveNickname_WithInvalidCharacters_ShouldFail(string nickname)
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("alphanumeric");
        _mockRepository.Verify(r => r.UpdateNicknameAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("Valid_Nickname")]
    [InlineData("User-123")]
    [InlineData("Test Name")]
    [InlineData("Player_2024")]
    public async Task ValidateAndSaveNickname_WithValidCharacters_ShouldSucceed(string nickname)
    {
        // Arrange
        var userId = 1;
        _mockRepository.Setup(r => r.IsNicknameAvailableAsync(nickname, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateNicknameAsync(userId, nickname), Times.Once);
    }

    [Fact]
    public async Task ValidateAndSaveNickname_ShouldTrimWhitespace()
    {
        // Arrange
        var userId = 1;
        var nickname = "  TrimmedNickname  ";
        var trimmed = "TrimmedNickname";
        _mockRepository.Setup(r => r.IsNicknameAvailableAsync(trimmed, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateNicknameAsync(userId, trimmed), Times.Once);
    }

    [Fact]
    public async Task ValidateAndSaveNickname_WithDuplicateNickname_ShouldFail()
    {
        // Arrange
        var userId = 1;
        var nickname = "TakenNickname";
        _mockRepository.Setup(r => r.IsNicknameAvailableAsync(nickname, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already taken");
        _mockRepository.Verify(r => r.UpdateNicknameAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAndSaveNickname_WithEmptyNickname_ShouldFail()
    {
        // Arrange
        var userId = 1;
        var nickname = "";

        // Act
        var result = await _nicknameService.ValidateAndSaveNicknameAsync(userId, nickname);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at least 2 characters");
    }
}
