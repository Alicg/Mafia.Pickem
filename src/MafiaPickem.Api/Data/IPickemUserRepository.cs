using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Data;

public interface IPickemUserRepository
{
    Task<PickemUser?> GetByTelegramIdAsync(long telegramId);
    Task<PickemUser?> GetByIdAsync(int id);
    Task<PickemUser> UpsertByTelegramIdAsync(long telegramId, string? photoUrl);
    Task UpdateNicknameAsync(int userId, string nickname);
    Task<bool> IsNicknameAvailableAsync(string nickname, int? excludeUserId);
}
