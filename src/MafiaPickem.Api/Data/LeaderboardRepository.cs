using Dapper;
using MafiaPickem.Api.Models.Responses;

namespace MafiaPickem.Api.Data;

public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LeaderboardRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task UpdateLeaderboardAsync(int tournamentId)
    {
        using var connection = _connectionFactory.CreateConnection();

        // First, MERGE to update or insert leaderboard entries
        const string mergeSql = """
            MERGE INTO pickem.Leaderboard AS target
            USING (
                SELECT 
                    m.TournamentId,
                    p.UserId,
                    ISNULL(SUM(ps.TotalPoints), 0) AS TotalPoints,
                    SUM(CASE WHEN ps.TotalPoints > 0 THEN 1 ELSE 0 END) AS CorrectPredictions,
                    COUNT(p.Id) AS TotalPredictions
                FROM pickem.Prediction p
                INNER JOIN pickem.Match m ON p.MatchId = m.Id
                LEFT JOIN pickem.PredictionScore ps ON p.Id = ps.PredictionId
                WHERE m.TournamentId = @TournamentId
                GROUP BY m.TournamentId, p.UserId
            ) AS source
            ON target.TournamentId = source.TournamentId AND target.UserId = source.UserId
            WHEN MATCHED THEN
                UPDATE SET 
                    TotalPoints = source.TotalPoints,
                    CorrectPredictions = source.CorrectPredictions,
                    TotalPredictions = source.TotalPredictions,
                    DateUpdated = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (TournamentId, UserId, TotalPoints, CorrectPredictions, TotalPredictions, DateUpdated)
                VALUES (source.TournamentId, source.UserId, source.TotalPoints, source.CorrectPredictions, source.TotalPredictions, GETUTCDATE());
            """;

        await connection.ExecuteAsync(mergeSql, new { TournamentId = tournamentId });

        // Then, update ranks using ROW_NUMBER
        const string rankSql = """
            WITH RankedLeaderboard AS (
                SELECT 
                    Id,
                    ROW_NUMBER() OVER (ORDER BY TotalPoints DESC, UserId ASC) AS NewRank
                FROM pickem.Leaderboard
                WHERE TournamentId = @TournamentId
            )
            UPDATE l
            SET l.Rank = r.NewRank
            FROM pickem.Leaderboard l
            INNER JOIN RankedLeaderboard r ON l.Id = r.Id
            """;

        await connection.ExecuteAsync(rankSql, new { TournamentId = tournamentId });
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(int tournamentId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT 
                l.UserId,
                l.Rank,
                u.GameNickname AS DisplayName,
                u.PhotoUrl,
                l.TotalPoints,
                l.CorrectPredictions,
                l.TotalPredictions
            FROM pickem.Leaderboard l
            INNER JOIN pickem.PickemUser u ON l.UserId = u.Id
            WHERE l.TournamentId = @TournamentId
            ORDER BY l.Rank
            """;

        var entries = (await connection.QueryAsync<LeaderboardEntryDto>(sql, new
        {
            TournamentId = tournamentId
        })).ToList();

        return new LeaderboardResponse
        {
            Entries = entries
        };
    }
}
