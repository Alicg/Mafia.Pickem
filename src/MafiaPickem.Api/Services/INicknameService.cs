namespace MafiaPickem.Api.Services;

public interface INicknameService
{
    Task<NicknameValidationResult> ValidateAndSaveNicknameAsync(int userId, string nickname);
}

public class NicknameValidationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static NicknameValidationResult Success() => new() { IsSuccess = true };
    public static NicknameValidationResult Fail(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
