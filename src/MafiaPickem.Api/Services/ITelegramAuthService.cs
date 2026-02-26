namespace MafiaPickem.Api.Services;

public interface ITelegramAuthService
{
    TelegramAuthResult? ValidateInitData(string initData);
}

public class TelegramAuthResult
{
    public long TelegramId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? PhotoUrl { get; set; }
}
