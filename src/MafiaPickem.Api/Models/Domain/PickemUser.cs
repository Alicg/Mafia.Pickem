namespace MafiaPickem.Api.Models.Domain;

public class PickemUser
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string GameNickname { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime DateCreated { get; set; }
}
