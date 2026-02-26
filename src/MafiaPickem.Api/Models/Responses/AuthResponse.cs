namespace MafiaPickem.Api.Models.Responses;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserProfileResponse User { get; set; } = null!;
}
