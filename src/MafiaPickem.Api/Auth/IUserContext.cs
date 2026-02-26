using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Auth;

public interface IUserContext
{
    int UserId { get; }
    long TelegramId { get; }
    string? GameNickname { get; }
    bool IsRegistered { get; }
    bool IsAdmin { get; }
    void Set(PickemUser user, bool isAdmin);
}
