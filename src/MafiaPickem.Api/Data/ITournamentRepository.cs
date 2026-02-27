using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Data;

public interface ITournamentRepository
{
    Task<IEnumerable<Tournament>> GetActiveAsync();
    Task<Tournament?> GetByIdAsync(int id);
    Task<Tournament> CreateAsync(string name, string? description, string? imageUrl);
}
