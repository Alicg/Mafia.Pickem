using System.Text.RegularExpressions;
using MafiaPickem.Api.Data;

namespace MafiaPickem.Api.Services;

public partial class NicknameService : INicknameService
{
    private readonly IPickemUserRepository _userRepository;
    
    private const int MinLength = 2;
    private const int MaxLength = 30;

    [GeneratedRegex(@"^[a-zA-Z0-9 _-]+$")]
    private static partial Regex NicknameRegex();

    public NicknameService(IPickemUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<NicknameValidationResult> ValidateAndSaveNicknameAsync(int userId, string nickname)
    {
        // Trim whitespace
        nickname = nickname.Trim();

        // Check length
        if (nickname.Length < MinLength)
        {
            return NicknameValidationResult.Fail($"Nickname must be at least {MinLength} characters long.");
        }

        if (nickname.Length > MaxLength)
        {
            return NicknameValidationResult.Fail($"Nickname must be maximum {MaxLength} characters long.");
        }

        // Check allowed characters
        if (!NicknameRegex().IsMatch(nickname))
        {
            return NicknameValidationResult.Fail("Nickname can only contain alphanumeric characters, spaces, underscores, and hyphens.");
        }

        // Check uniqueness
        var isAvailable = await _userRepository.IsNicknameAvailableAsync(nickname, userId);
        if (!isAvailable)
        {
            return NicknameValidationResult.Fail("This nickname is already taken.");
        }

        // Save nickname
        await _userRepository.UpdateNicknameAsync(userId, nickname);

        return NicknameValidationResult.Success();
    }
}
