using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Data;

public interface ITournamentParticipantRepository
{
    Task<TournamentParticipant?> GetByTournamentAndUserAsync(int tournamentId, int userId);
    Task<TournamentParticipant> CreateAsync(int tournamentId, int userId, string teamName);
    Task<bool> HasTeamSelectionAsync(int tournamentId, int userId);
}