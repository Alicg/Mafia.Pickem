namespace MafiaPickem.Api.Data;

public interface ITournamentOperatorRepository
{
    Task AddRangeAsync(int tournamentId, IReadOnlyCollection<string> operatorUsernames);
    Task<bool> IsOperatorAsync(int tournamentId, string? telegramUsername);
    Task<IReadOnlyCollection<int>> GetTournamentIdsByUsernameAsync(string? telegramUsername);
}