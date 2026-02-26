namespace MafiaPickem.Api.Models.Responses;

public class UserProfileResponse
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string? GameNickname { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsRegistered { get; set; }
    public bool IsAdmin { get; set; }
}
