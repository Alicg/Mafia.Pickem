using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Data;

public interface ITournamentRepository
{
    Task<IEnumerable<Tournament>> GetActiveAsync();
    Task<Tournament?> GetByIdAsync(int id);
    Task<Tournament> CreateAsync(string name, string? description, string? imageUrl, IReadOnlyCollection<string> teams, bool visibleOnHomePage);
    Task<Tournament> UpdateAsync(int id, string name, string? description, string? imageUrl, IReadOnlyCollection<string> teams, bool visibleOnHomePage);
    Task DeleteAsync(int id);
}
