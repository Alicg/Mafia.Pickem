using Dapper;
using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Data;

public class TournamentParticipantRepository : ITournamentParticipantRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TournamentParticipantRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TournamentParticipant?> GetByTournamentAndUserAsync(int tournamentId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, TournamentId, UserId, TeamName, DateCreated
            FROM pickem.TournamentParticipant
            WHERE TournamentId = @TournamentId AND UserId = @UserId
            """;

        return await connection.QuerySingleOrDefaultAsync<TournamentParticipant>(sql, new
        {
            TournamentId = tournamentId,
            UserId = userId
        });
    }

    public async Task<TournamentParticipant> CreateAsync(int tournamentId, int userId, string teamName)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO pickem.TournamentParticipant (TournamentId, UserId, TeamName)
            VALUES (@TournamentId, @UserId, @TeamName);

            SELECT Id, TournamentId, UserId, TeamName, DateCreated
            FROM pickem.TournamentParticipant
            WHERE TournamentId = @TournamentId AND UserId = @UserId;
            """;

        return await connection.QuerySingleAsync<TournamentParticipant>(sql, new
        {
            TournamentId = tournamentId,
            UserId = userId,
            TeamName = teamName
        });
    }

    public async Task<bool> HasTeamSelectionAsync(int tournamentId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM pickem.TournamentParticipant
            WHERE TournamentId = @TournamentId AND UserId = @UserId
            """;

        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            TournamentId = tournamentId,
            UserId = userId
        });

        return count > 0;
    }
}