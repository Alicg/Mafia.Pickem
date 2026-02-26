using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Auth;

public class UserContext : IUserContext
{
    public int UserId { get; private set; }
    public long TelegramId { get; private set; }
    public string? GameNickname { get; private set; }
    public bool IsRegistered { get; private set; }
    public bool IsAdmin { get; private set; }

    public void Set(PickemUser user, bool isAdmin)
    {
        UserId = user.Id;
        TelegramId = user.TelegramId;
        GameNickname = user.GameNickname;
        IsRegistered = !user.GameNickname.StartsWith("_unregistered_");
        IsAdmin = isAdmin;
    }
}
