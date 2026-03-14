namespace MafiaPickem.Api.Data;

public interface ITournamentOperatorRepository
{
    Task AddRangeAsync(int tournamentId, IReadOnlyCollection<string> operatorUsernames);
    Task<IReadOnlyCollection<string>> GetByTournamentIdAsync(int tournamentId);
    Task<bool> IsOperatorAsync(int tournamentId, string? telegramUsername);
    Task<IReadOnlyCollection<int>> GetTournamentIdsByUsernameAsync(string? telegramUsername);
    Task ReplaceAsync(int tournamentId, IReadOnlyCollection<string> operatorUsernames);
}