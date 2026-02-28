using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Services;

public class MatchStateService : IMatchStateService
{
    private readonly IMatchRepository _matchRepository;

    public MatchStateService(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Match> OpenMatchAsync(int matchId)
    {
        var match = await GetMatchOrThrowAsync(matchId);

        if (match.State != MatchState.Upcoming)
        {
            throw new InvalidOperationException($"Cannot transition match from {match.State} to Open");
        }

        await _matchRepository.UpdateStateAsync(matchId, MatchState.Open);
        return (await _matchRepository.GetByIdAsync(matchId))!;
    }

    public async Task<Match> RevertToUpcomingAsync(int matchId)
    {
        var match = await GetMatchOrThrowAsync(matchId);

        if (match.State != MatchState.Open)
        {
            throw new InvalidOperationException($"Cannot transition match from {match.State} to Upcoming");
        }

        await _matchRepository.UpdateStateAsync(matchId, MatchState.Upcoming);
        return (await _matchRepository.GetByIdAsync(matchId))!;
    }

    public async Task<Match> LockMatchAsync(int matchId)
    {
        var match = await GetMatchOrThrowAsync(matchId);

        if (match.State != MatchState.Open)
        {
            throw new InvalidOperationException($"Cannot transition match from {match.State} to Locked");
        }

        await _matchRepository.UpdateStateAsync(matchId, MatchState.Locked);
        return (await _matchRepository.GetByIdAsync(matchId))!;
    }

    public async Task<Match> ResolveMatchAsync(int matchId)
    {
        var match = await GetMatchOrThrowAsync(matchId);

        if (match.State != MatchState.Locked)
        {
            throw new InvalidOperationException($"Cannot transition match from {match.State} to Resolved");
        }

        await _matchRepository.UpdateStateAsync(matchId, MatchState.Resolved);
        return (await _matchRepository.GetByIdAsync(matchId))!;
    }

    public async Task<Match> CancelMatchAsync(int matchId)
    {
        var match = await GetMatchOrThrowAsync(matchId);

        if (match.State == MatchState.Resolved)
        {
            throw new InvalidOperationException($"Cannot transition match from {match.State} to Canceled");
        }

        await _matchRepository.UpdateStateAsync(matchId, MatchState.Canceled);
        return (await _matchRepository.GetByIdAsync(matchId))!;
    }

    private async Task<Match> GetMatchOrThrowAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            throw new InvalidOperationException($"Match with ID {matchId} not found");
        }
        return match;
    }
}
