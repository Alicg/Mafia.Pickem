using Dapper;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Overlay;
using System.Text.Json;

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
            SELECT Id, Name, Description, ImageUrl, TeamsJson, Active, VisibleOnHomePage, ShowTeamSelection, OverlaySettingsJson, DateCreated
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
            SELECT Id, Name, Description, ImageUrl, TeamsJson, Active, VisibleOnHomePage, ShowTeamSelection, OverlaySettingsJson, DateCreated
            FROM pickem.Tournament
            WHERE Id = @Id
            """;

        return await connection.QuerySingleOrDefaultAsync<Tournament>(sql, new { Id = id });
    }

    public async Task<Tournament> CreateAsync(string name, string? description, string? imageUrl, IReadOnlyCollection<string> teams, bool visibleOnHomePage, bool showTeamSelection, TournamentOverlaySettings overlaySettings)
    {
        using var connection = _connectionFactory.CreateConnection();
        var teamsJson = JsonSerializer.Serialize(teams);
        var overlaySettingsJson = TournamentOverlaySettingsSerializer.Serialize(overlaySettings);

        const string sql = """
            INSERT INTO pickem.Tournament (Name, Description, ImageUrl, TeamsJson, VisibleOnHomePage, ShowTeamSelection, OverlaySettingsJson)
            VALUES (@Name, @Description, @ImageUrl, @TeamsJson, @VisibleOnHomePage, @ShowTeamSelection, @OverlaySettingsJson);
            SELECT CAST(SCOPE_IDENTITY() AS int);
            """;

        var id = await connection.QuerySingleAsync<int>(sql, new { Name = name, Description = description, ImageUrl = imageUrl, TeamsJson = teamsJson, VisibleOnHomePage = visibleOnHomePage, ShowTeamSelection = showTeamSelection, OverlaySettingsJson = overlaySettingsJson });

        return (await GetByIdAsync(id))!;
    }

    public async Task<Tournament> UpdateAsync(int id, string name, string? description, string? imageUrl, IReadOnlyCollection<string> teams, bool visibleOnHomePage, bool showTeamSelection, TournamentOverlaySettings overlaySettings)
    {
        using var connection = _connectionFactory.CreateConnection();
        var teamsJson = JsonSerializer.Serialize(teams);
        var overlaySettingsJson = TournamentOverlaySettingsSerializer.Serialize(overlaySettings);

        const string sql = """
            UPDATE pickem.Tournament
            SET Name = @Name,
                Description = @Description,
                ImageUrl = @ImageUrl,
                TeamsJson = @TeamsJson,
                VisibleOnHomePage = @VisibleOnHomePage,
                ShowTeamSelection = @ShowTeamSelection,
                OverlaySettingsJson = @OverlaySettingsJson
            WHERE Id = @Id
            """;

        await connection.ExecuteAsync(sql, new { Id = id, Name = name, Description = description, ImageUrl = imageUrl, TeamsJson = teamsJson, VisibleOnHomePage = visibleOnHomePage, ShowTeamSelection = showTeamSelection, OverlaySettingsJson = overlaySettingsJson });

        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            DELETE FROM pickem.Tournament
            WHERE Id = @Id
            """;

        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
