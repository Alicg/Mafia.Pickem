using Dapper;
using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Data;

public class TournamentRepository : ITournamentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TournamentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Tournament>> GetActiveAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, Name, Description, ImageUrl, Active, DateCreated
            FROM pickem.Tournament
            WHERE Active = 1
            ORDER BY DateCreated DESC
            """;

        return await connection.QueryAsync<Tournament>(sql);
    }

    public async Task<Tournament?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id, Name, Description, ImageUrl, Active, DateCreated
            FROM pickem.Tournament
            WHERE Id = @Id
            """;

        return await connection.QuerySingleOrDefaultAsync<Tournament>(sql, new { Id = id });
    }

    public async Task<Tournament> CreateAsync(string name, string? description, string? imageUrl)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO pickem.Tournament (Name, Description, ImageUrl)
            VALUES (@Name, @Description, @ImageUrl);
            SELECT CAST(SCOPE_IDENTITY() AS int);
            """;

        var id = await connection.QuerySingleAsync<int>(sql, new { Name = name, Description = description, ImageUrl = imageUrl });

        return (await GetByIdAsync(id))!;
    }
}
