using Dapper;
using MafiaPickem.Api.Utils;

namespace MafiaPickem.Api.Data;

public class TournamentOperatorRepository : ITournamentOperatorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TournamentOperatorRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddRangeAsync(int tournamentId, IReadOnlyCollection<string> operatorUsernames)
    {
        if (operatorUsernames.Count == 0)
        {
            return;
        }

        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO pickem.TournamentOperator (TournamentId, OperatorUsername)
            VALUES (@TournamentId, @OperatorUsername)
            """;

        var parameters = operatorUsernames
            .Select(TelegramUsernameNormalizer.NormalizeMention)
            .Where(username => username != null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(operatorUsername => new { TournamentId = tournamentId, OperatorUsername = operatorUsername });

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<bool> IsOperatorAsync(int tournamentId, string? telegramUsername)
    {
        var normalizedUsername = TelegramUsernameNormalizer.NormalizeMention(telegramUsername);
        if (normalizedUsername == null)
        {
            return false;
        }

        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM pickem.TournamentOperator
            WHERE TournamentId = @TournamentId AND OperatorUsername = @OperatorUsername
            """;

        var count = await connection.ExecuteScalarAsync<int>(sql, new { TournamentId = tournamentId, OperatorUsername = normalizedUsername });
        return count > 0;
    }

    public async Task<IReadOnlyCollection<int>> GetTournamentIdsByUsernameAsync(string? telegramUsername)
    {
        var normalizedUsername = TelegramUsernameNormalizer.NormalizeMention(telegramUsername);
        if (normalizedUsername == null)
        {
            return Array.Empty<int>();
        }

        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT TournamentId
            FROM pickem.TournamentOperator
            WHERE OperatorUsername = @OperatorUsername
            """;

        var result = await connection.QueryAsync<int>(sql, new { OperatorUsername = normalizedUsername });
        return result.ToArray();
    }
}