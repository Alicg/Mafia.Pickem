using Dapper;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Data;

public class MatchRepository : IMatchRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MatchRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Match?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, TournamentId, ExternalMatchRef, GameNumber, TableNumber, State, 
                   DateCreated, DateOpened, DateLocked, DateResolved
            FROM pickem.Match
            WHERE Id = @Id
            """;

        return await connection.QuerySingleOrDefaultAsync<Match>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Match>> GetByTournamentIdAsync(int tournamentId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, TournamentId, ExternalMatchRef, GameNumber, TableNumber, State, 
                   DateCreated, DateOpened, DateLocked, DateResolved
            FROM pickem.Match
            WHERE TournamentId = @TournamentId
            ORDER BY Id DESC
            """;

        return await connection.QueryAsync<Match>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Match?> GetCurrentMatchByTournamentIdAsync(int tournamentId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT TOP 1 Id, TournamentId, ExternalMatchRef, GameNumber, TableNumber, State, 
                   DateCreated, DateOpened, DateLocked, DateResolved
            FROM pickem.Match
            WHERE TournamentId = @TournamentId 
              AND State <> @CanceledState
            ORDER BY Id DESC
            """;

        return await connection.QuerySingleOrDefaultAsync<Match>(sql, new
        {
            TournamentId = tournamentId,
            CanceledState = (byte)MatchState.Canceled
        });
    }

    public async Task<Match> CreateAsync(int tournamentId, int gameNumber, int? tableNumber, string? externalMatchRef)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO pickem.Match (TournamentId, GameNumber, TableNumber, ExternalMatchRef, State, DateCreated)
            VALUES (@TournamentId, @GameNumber, @TableNumber, @ExternalMatchRef, @State, GETUTCDATE());

            SELECT Id, TournamentId, ExternalMatchRef, GameNumber, TableNumber, State, 
                   DateCreated, DateOpened, DateLocked, DateResolved
            FROM pickem.Match
            WHERE Id = SCOPE_IDENTITY();
            """;

        return await connection.QuerySingleAsync<Match>(sql, new
        {
            TournamentId = tournamentId,
            GameNumber = gameNumber,
            TableNumber = tableNumber,
            ExternalMatchRef = externalMatchRef,
            State = (byte)MatchState.Upcoming
        });
    }

    public async Task UpdateStateAsync(int matchId, MatchState newState)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Determine which date field to update based on the new state
        var dateColumn = newState switch
        {
            MatchState.Open => "DateOpened",
            MatchState.Locked => "DateLocked",
            MatchState.Resolved => "DateResolved",
            _ => null
        };

        var sql = dateColumn != null
            ? $"""
                UPDATE pickem.Match
                SET State = @NewState, {dateColumn} = GETUTCDATE()
                WHERE Id = @MatchId
                """
            : """
                UPDATE pickem.Match
                SET State = @NewState
                WHERE Id = @MatchId
                """;

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            NewState = (byte)newState
        });
    }

    public async Task<IEnumerable<Match>> GetByTournamentAndStateAsync(int tournamentId, params MatchState[] states)
    {
        using var connection = _connectionFactory.CreateConnection();

        var stateBytes = states.Select(s => (byte)s).ToArray();

        const string sql = """
            SELECT Id, TournamentId, ExternalMatchRef, GameNumber, TableNumber, State, 
                   DateCreated, DateOpened, DateLocked, DateResolved
            FROM pickem.Match
            WHERE TournamentId = @TournamentId
              AND State IN @States
            ORDER BY Id DESC
            """;

        return await connection.QueryAsync<Match>(sql, new
        {
            TournamentId = tournamentId,
            States = stateBytes
        });
    }
}
