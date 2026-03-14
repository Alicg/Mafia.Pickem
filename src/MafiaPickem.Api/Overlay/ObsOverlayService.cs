using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Domain;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.State;

namespace MafiaPickem.Api.Overlay;

public class ObsOverlayService : IObsOverlayService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchStateBlobReader _matchStateBlobReader;

    public ObsOverlayService(
        IMatchRepository matchRepository,
        IMatchStateBlobReader matchStateBlobReader)
    {
        _matchRepository = matchRepository;
        _matchStateBlobReader = matchStateBlobReader;
    }

    public async Task<ObsOverlayPayload> GetOverlayPayloadAsync(int tournamentId)
    {
        var matches = (await _matchRepository.GetByTournamentAndStateAsync(
            tournamentId,
            MatchState.Open,
            MatchState.Locked,
            MatchState.FirstVoted,
            MatchState.Resolved)).ToList();

        var selectedMatch = SelectMatch(matches);
        if (selectedMatch == null)
        {
            return CreateBasePayload(tournamentId, "no-match", "No open, locked, or resolved match is available for this tournament.");
        }

        var blobState = await _matchStateBlobReader.ReadStateAsync(selectedMatch.Id);
        if (blobState == null)
        {
            return CreateBasePayload(
                tournamentId,
                "missing-state",
                $"Published state was not found for match {selectedMatch.Id}.",
                selectedMatch);
        }

        var seatCount = Math.Max(blobState.TableSize, 10);
        var votedOutLookup = (blobState.VotedOutVotes ?? new List<SlotVoteEntry>())
            .ToDictionary(entry => entry.Slot);
        var resolvedSlots = blobState.MatchResult?.VotedOutSlots?.Distinct().OrderBy(slot => slot).ToList() ?? new List<int>();
        var lastRoundLookup = (blobState.LastRoundVotes ?? new List<LastRoundVoteEntry>())
            .ToDictionary(entry => entry.LastRound);

        var payload = CreateBasePayload(tournamentId, "ready", string.Empty, selectedMatch);
        payload.MatchState = blobState.State;
        payload.MatchStateLabel = GetStateLabel(blobState.State);
        payload.UpdatedAt = blobState.UpdatedAt;
        payload.TotalPredictions = blobState.TotalPredictions;
        payload.WinningSide = blobState.MatchResult?.WinningSide;
        payload.ResolvedSlots = resolvedSlots;
        payload.ResolvedLastRound = blobState.MatchResult?.LastRound;
        payload.RedSide = new OverlaySideStat
        {
            Count = blobState.WinnerVotes?.Town.Count ?? 0,
            Percent = blobState.WinnerVotes?.Town.Percent ?? 0m
        };
        payload.BlackSide = new OverlaySideStat
        {
            Count = blobState.WinnerVotes?.Mafia.Count ?? 0,
            Percent = blobState.WinnerVotes?.Mafia.Percent ?? 0m
        };
        payload.SeatVotes = Enumerable.Range(1, seatCount)
            .Select(slot =>
            {
                votedOutLookup.TryGetValue(slot, out var vote);
                return new OverlaySeatVote
                {
                    Slot = slot,
                    Count = vote?.Count ?? 0,
                    Percent = vote?.Percent ?? 0m,
                    IsResolved = resolvedSlots.Contains(slot)
                };
            })
            .ToList();

        payload.LastRoundVotes = new byte[] { 1, 2, 3, 4, 5 }
            .Select(lr =>
            {
                lastRoundLookup.TryGetValue(lr, out var vote);
                return new OverlayLastRoundVote
                {
                    LastRound = lr,
                    Label = GetLastRoundLabel(lr),
                    Count = vote?.Count ?? 0,
                    Percent = vote?.Percent ?? 0m
                };
            })
            .ToList();

        return payload;
    }

    private static Match? SelectMatch(IEnumerable<Match> matches)
    {
        var openMatch = matches
            .Where(match => match.State == MatchState.Open)
            .OrderByDescending(match => match.DateOpened ?? match.DateCreated)
            .ThenByDescending(match => match.Id)
            .FirstOrDefault();

        if (openMatch != null)
        {
            return openMatch;
        }

        var lockedMatch = matches
            .Where(match => match.State == MatchState.Locked)
            .OrderByDescending(match => match.DateLocked ?? match.DateCreated)
            .ThenByDescending(match => match.Id)
            .FirstOrDefault();

        if (lockedMatch != null)
        {
            return lockedMatch;
        }

        // Prefer FirstVoted over Resolved (game still in progress)
        var firstVotedMatch = matches
            .Where(match => match.State == MatchState.FirstVoted)
            .OrderByDescending(match => match.DateLocked ?? match.DateCreated)
            .ThenByDescending(match => match.Id)
            .FirstOrDefault();

        if (firstVotedMatch != null)
        {
            return firstVotedMatch;
        }

        return matches
            .Where(match => match.State == MatchState.Resolved)
            .OrderByDescending(match => match.DateResolved ?? match.DateCreated)
            .ThenByDescending(match => match.Id)
            .FirstOrDefault();
    }

    private static ObsOverlayPayload CreateBasePayload(int tournamentId, string status, string message, Match? match = null)
    {
        return new ObsOverlayPayload
        {
            Status = status,
            Message = message,
            TournamentId = tournamentId,
            MatchId = match?.Id,
            GameNumber = match?.GameNumber,
            TableNumber = match?.TableNumber,
            MatchState = match?.State.ToString() ?? string.Empty,
            MatchStateLabel = match == null ? string.Empty : GetStateLabel(match.State.ToString()),
            SeatVotes = Enumerable.Range(1, 10)
                .Select(slot => new OverlaySeatVote { Slot = slot })
                .ToList()
        };
    }

    private static string GetStateLabel(string state)
    {
        return state switch
        {
            nameof(MatchState.Open) => "Open",
            nameof(MatchState.Resolved) => "Resolved",
            nameof(MatchState.Locked) => "Locked",
            nameof(MatchState.FirstVoted) => "FirstVoted",
            nameof(MatchState.Upcoming) => "Upcoming",
            nameof(MatchState.Canceled) => "Canceled",
            _ => state
        };
    }

    private static string GetLastRoundLabel(byte lastRound)
    {
        return lastRound switch
        {
            1 => "Сухая",
            2 => "Другое",
            3 => "3в3",
            4 => "2в2",
            5 => "1в1",
            _ => lastRound.ToString()
        };
    }
}
