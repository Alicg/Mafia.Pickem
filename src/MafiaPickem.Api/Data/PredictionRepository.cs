using Dapper;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Responses;

namespace MafiaPickem.Api.Data;

public class PredictionRepository : IPredictionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PredictionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Prediction?> GetByMatchAndUserAsync(int matchId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, MatchId, UserId, PredictedWinner, PredictedVotedOut, DateCreated, DateUpdated
            FROM pickem.Prediction
            WHERE MatchId = @MatchId AND UserId = @UserId
            """;

        return await connection.QuerySingleOrDefaultAsync<Prediction>(sql, new { MatchId = matchId, UserId = userId });
    }

    public async Task<List<Prediction>> GetByTournamentAndUserAsync(int tournamentId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT p.Id, p.MatchId, p.UserId, p.PredictedWinner, p.PredictedVotedOut, p.DateCreated, p.DateUpdated
            FROM pickem.Prediction p
            INNER JOIN pickem.Match m ON m.Id = p.MatchId
            WHERE m.TournamentId = @TournamentId AND p.UserId = @UserId
            """;

        var results = await connection.QueryAsync<Prediction>(sql, new { TournamentId = tournamentId, UserId = userId });
        return results.ToList();
    }

    public async Task UpsertAsync(int matchId, int userId, byte predictedWinner, byte predictedVotedOut)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            MERGE INTO pickem.Prediction AS target
            USING (SELECT @MatchId AS MatchId, @UserId AS UserId) AS source
            ON target.MatchId = source.MatchId AND target.UserId = source.UserId
            WHEN MATCHED THEN
                UPDATE SET 
                    PredictedWinner = @PredictedWinner,
                    PredictedVotedOut = @PredictedVotedOut,
                    DateUpdated = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (MatchId, UserId, PredictedWinner, PredictedVotedOut, DateCreated)
                VALUES (@MatchId, @UserId, @PredictedWinner, @PredictedVotedOut, GETUTCDATE());
            """;

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            UserId = userId,
            PredictedWinner = predictedWinner,
            PredictedVotedOut = predictedVotedOut
        });
    }

    public async Task<VoteStatsDto> GetVoteStatsAsync(int matchId)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Get total votes and winner percentages
        const string statsSql = """
            SELECT 
                COUNT(*) AS TotalVotes,
                CAST(SUM(CASE WHEN PredictedWinner = 0 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS TownPercentage,
                CAST(SUM(CASE WHEN PredictedWinner = 1 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS MafiaPercentage
            FROM pickem.Prediction
            WHERE MatchId = @MatchId
            """;

        var stats = await connection.QuerySingleAsync<(int TotalVotes, decimal TownPercentage, decimal MafiaPercentage)>(
            statsSql, new { MatchId = matchId });

        // Get slot vote counts
        const string slotsSql = """
            SELECT 
                PredictedVotedOut AS Slot,
                COUNT(*) AS Count,
                CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM pickem.Prediction WHERE MatchId = @MatchId) AS DECIMAL(5,2)) AS Percentage
            FROM pickem.Prediction
            WHERE MatchId = @MatchId
            GROUP BY PredictedVotedOut
            ORDER BY PredictedVotedOut
            """;

        var slots = (await connection.QueryAsync<SlotVoteDto>(slotsSql, new { MatchId = matchId })).ToList();

        return new VoteStatsDto
        {
            TotalVotes = stats.TotalVotes,
            TownPercentage = stats.TownPercentage,
            MafiaPercentage = stats.MafiaPercentage,
            SlotVotes = slots
        };
    }

    public async Task<int> GetTotalVotesAsync(int matchId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(*)
            FROM pickem.Prediction
            WHERE MatchId = @MatchId
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new { MatchId = matchId });
    }

    public async Task<int> GetCorrectWinnerVotesAsync(int matchId, byte winningSide)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(*)
            FROM pickem.Prediction
            WHERE MatchId = @MatchId AND PredictedWinner = @WinningSide
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new { MatchId = matchId, WinningSide = winningSide });
    }

    public async Task<int> GetCorrectVotedOutVotesAsync(int matchId, string correctVotedOutCsv)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(*)
            FROM pickem.Prediction p
            WHERE p.MatchId = @MatchId 
              AND (
                  @CorrectVotedOutCsv = CAST(p.PredictedVotedOut AS NVARCHAR(10))
                  OR @CorrectVotedOutCsv LIKE CAST(p.PredictedVotedOut AS NVARCHAR(10)) + ',%'
                  OR @CorrectVotedOutCsv LIKE '%,' + CAST(p.PredictedVotedOut AS NVARCHAR(10)) + ',%'
                  OR @CorrectVotedOutCsv LIKE '%,' + CAST(p.PredictedVotedOut AS NVARCHAR(10))
              )
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new { MatchId = matchId, CorrectVotedOutCsv = correctVotedOutCsv });
    }

    public async Task InsertScoresAsync(int matchId, int totalVotes, int correctWinnerVotes, int correctVotedOutVotes)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO pickem.PredictionScore (PredictionId, WinnerPoints, VotedOutPoints, TotalPoints, 
                                          TotalVotes, CorrectWinnerVotes, CorrectVotedOutVotes, DateCalculated)
            SELECT 
                p.Id AS PredictionId,
                CASE 
                    WHEN p.PredictedWinner = mr.WinningSide AND @CorrectWinnerVotes > 0 
                    THEN CAST(@TotalVotes AS DECIMAL(12,4)) / @CorrectWinnerVotes * 10
                    ELSE 0
                END AS WinnerPoints,
                CASE 
                    WHEN (
                        mr.CorrectVotedOutCsv = CAST(p.PredictedVotedOut AS NVARCHAR(10))
                        OR mr.CorrectVotedOutCsv LIKE CAST(p.PredictedVotedOut AS NVARCHAR(10)) + ',%'
                        OR mr.CorrectVotedOutCsv LIKE '%,' + CAST(p.PredictedVotedOut AS NVARCHAR(10)) + ',%'
                        OR mr.CorrectVotedOutCsv LIKE '%,' + CAST(p.PredictedVotedOut AS NVARCHAR(10))
                    ) AND @CorrectVotedOutVotes > 0
                    THEN CAST(@TotalVotes AS DECIMAL(12,4)) / @CorrectVotedOutVotes * 20
                    ELSE 0
                END AS VotedOutPoints,
                CASE 
                    WHEN p.PredictedWinner = mr.WinningSide AND @CorrectWinnerVotes > 0 
                    THEN CAST(@TotalVotes AS DECIMAL(12,4)) / @CorrectWinnerVotes * 10
                    ELSE 0
                END + 
                CASE 
                    WHEN (
                        mr.CorrectVotedOutCsv = CAST(p.PredictedVotedOut AS NVARCHAR(10))
                        OR mr.CorrectVotedOutCsv LIKE CAST(p.PredictedVotedOut AS NVARCHAR(10)) + ',%'
                        OR mr.CorrectVotedOutCsv LIKE '%,' + CAST(p.PredictedVotedOut AS NVARCHAR(10)) + ',%'
                        OR mr.CorrectVotedOutCsv LIKE '%,' + CAST(p.PredictedVotedOut AS NVARCHAR(10))
                    ) AND @CorrectVotedOutVotes > 0
                    THEN CAST(@TotalVotes AS DECIMAL(12,4)) / @CorrectVotedOutVotes * 20
                    ELSE 0
                END AS TotalPoints,
                @TotalVotes AS TotalVotes,
                @CorrectWinnerVotes AS CorrectWinnerVotes,
                @CorrectVotedOutVotes AS CorrectVotedOutVotes,
                GETUTCDATE() AS DateCalculated
            FROM pickem.Prediction p
            INNER JOIN pickem.MatchResult mr ON p.MatchId = mr.MatchId
            WHERE p.MatchId = @MatchId
            """;

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            TotalVotes = totalVotes,
            CorrectWinnerVotes = correctWinnerVotes,
            CorrectVotedOutVotes = correctVotedOutVotes
        });
    }

    public async Task<PredictionScore?> GetScoreByPredictionIdAsync(int predictionId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, PredictionId, WinnerPoints, VotedOutPoints, TotalPoints, 
                   TotalVotes, CorrectWinnerVotes, CorrectVotedOutVotes, DateCalculated
            FROM pickem.PredictionScore
            WHERE PredictionId = @PredictionId
            """;

        return await connection.QuerySingleOrDefaultAsync<PredictionScore>(sql, new { PredictionId = predictionId });
    }

    public async Task<(byte WinningSide, string CorrectVotedOutCsv)?> GetMatchResultAsync(int matchId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT WinningSide, CorrectVotedOutCsv
            FROM pickem.MatchResult
            WHERE MatchId = @MatchId
            """;

        var row = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { MatchId = matchId });
        if (row == null) return null;

        return ((byte)row.WinningSide, (string)row.CorrectVotedOutCsv);
    }

    public async Task SaveMatchResultAsync(int matchId, byte winningSide, string correctVotedOutCsv)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            MERGE INTO pickem.MatchResult AS target
            USING (SELECT @MatchId AS MatchId) AS source
            ON target.MatchId = source.MatchId
            WHEN MATCHED THEN
                UPDATE SET 
                    WinningSide = @WinningSide,
                    CorrectVotedOutCsv = @CorrectVotedOutCsv,
                    DateResolved = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (MatchId, WinningSide, CorrectVotedOutCsv, DateResolved)
                VALUES (@MatchId, @WinningSide, @CorrectVotedOutCsv, GETUTCDATE());
            """;

        await connection.ExecuteAsync(sql, new
        {
            MatchId = matchId,
            WinningSide = winningSide,
            CorrectVotedOutCsv = correctVotedOutCsv
        });
    }

    public async Task DeleteScoresByMatchIdAsync(int matchId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM pickem.PredictionScore
            WHERE PredictionId IN (
                SELECT Id FROM pickem.Prediction WHERE MatchId = @MatchId
            )
            """;

        await connection.ExecuteAsync(sql, new { MatchId = matchId });
    }
}
