using Dapper;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Utils;

namespace MafiaPickem.Api.Data;

public class PickemUserRepository : IPickemUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PickemUserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PickemUser?> GetByTelegramIdAsync(long telegramId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, TelegramId, TelegramUsername, GameNickname, PhotoUrl, DateCreated
            FROM pickem.PickemUser
            WHERE TelegramId = @TelegramId
            """;

        return await connection.QuerySingleOrDefaultAsync<PickemUser>(sql, new { TelegramId = telegramId });
    }

    public async Task<PickemUser?> GetByTelegramUsernameAsync(string telegramUsername)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, TelegramId, TelegramUsername, GameNickname, PhotoUrl, DateCreated
            FROM pickem.PickemUser
            WHERE TelegramUsername = @TelegramUsername
            """;

        var normalizedUsername = TelegramUsernameNormalizer.Normalize(telegramUsername);
        if (normalizedUsername == null)
        {
            return null;
        }

        return await connection.QuerySingleOrDefaultAsync<PickemUser>(sql, new { TelegramUsername = normalizedUsername });
    }

    public async Task<PickemUser?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, TelegramId, TelegramUsername, GameNickname, PhotoUrl, DateCreated
            FROM pickem.PickemUser
            WHERE Id = @Id
            """;

        return await connection.QuerySingleOrDefaultAsync<PickemUser>(sql, new { Id = id });
    }

    public async Task<PickemUser> UpsertByTelegramIdAsync(long telegramId, string? telegramUsername, string? photoUrl)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Generate placeholder nickname for new users
        var placeholderNickname = $"_unregistered_{telegramId}";
        var normalizedUsername = TelegramUsernameNormalizer.Normalize(telegramUsername);

        const string sql = """
            MERGE INTO pickem.PickemUser AS target
            USING (SELECT @TelegramId AS TelegramId, @TelegramUsername AS TelegramUsername, @PhotoUrl AS PhotoUrl, @PlaceholderNickname AS GameNickname) AS source
            ON target.TelegramId = source.TelegramId
            WHEN MATCHED THEN
                UPDATE SET PhotoUrl = source.PhotoUrl,
                           TelegramUsername = source.TelegramUsername
            WHEN NOT MATCHED THEN
                INSERT (TelegramId, TelegramUsername, GameNickname, PhotoUrl, DateCreated)
                VALUES (source.TelegramId, source.TelegramUsername, source.GameNickname, source.PhotoUrl, GETUTCDATE());

            SELECT Id, TelegramId, TelegramUsername, GameNickname, PhotoUrl, DateCreated
            FROM pickem.PickemUser
            WHERE TelegramId = @TelegramId;
            """;

        return await connection.QuerySingleAsync<PickemUser>(sql, new
        {
            TelegramId = telegramId,
            TelegramUsername = normalizedUsername,
            PhotoUrl = photoUrl,
            PlaceholderNickname = placeholderNickname
        });
    }

    public async Task UpdateNicknameAsync(int userId, string nickname)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE pickem.PickemUser
            SET GameNickname = @Nickname
            WHERE Id = @UserId
            """;

        await connection.ExecuteAsync(sql, new { UserId = userId, Nickname = nickname });
    }

    public async Task<bool> IsNicknameAvailableAsync(string nickname, int? excludeUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM pickem.PickemUser
            WHERE GameNickname = @Nickname
              AND (@ExcludeUserId IS NULL OR Id != @ExcludeUserId)
            """;

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Nickname = nickname, ExcludeUserId = excludeUserId });
        return count == 0;
    }
}
