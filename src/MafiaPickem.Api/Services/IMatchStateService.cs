using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Services;

public interface IMatchStateService
{
    Task<Match> OpenMatchAsync(int matchId);
    Task<Match> RevertToUpcomingAsync(int matchId);
    Task<Match> LockMatchAsync(int matchId);
    Task<Match> ReopenMatchAsync(int matchId);
    Task<Match> ResolveMatchAsync(int matchId);
    Task<Match> UnresolveMatchAsync(int matchId);
}
