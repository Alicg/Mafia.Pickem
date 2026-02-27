using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Data;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(int id);
    Task<IEnumerable<Match>> GetByTournamentIdAsync(int tournamentId);
    Task<Match?> GetCurrentMatchByTournamentIdAsync(int tournamentId);
    Task<Match> CreateAsync(int tournamentId, int gameNumber, int? tableNumber, string? externalMatchRef);
    Task UpdateStateAsync(int matchId, MatchState newState);
    Task<IEnumerable<Match>> GetByTournamentAndStateAsync(int tournamentId, params MatchState[] states);
}
