using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;
using MafiaPickem.Api.State;

namespace MafiaPickem.Api.Services;

public class StatePublishService : IStatePublishService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IMatchStateBlobWriter _blobWriter;

    private static readonly TimeSpan _throttleInterval = TimeSpan.FromSeconds(10);

    public StatePublishService(
        IMatchRepository matchRepository,
        IPredictionRepository predictionRepository,
        IMatchStateBlobWriter blobWriter)
    {
        _matchRepository = matchRepository;
        _predictionRepository = predictionRepository;
        _blobWriter = blobWriter;
    }

    public async Task PublishMatchStateAsync(int matchId, bool forcePublish = false)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            throw new InvalidOperationException($"Match {matchId} not found");
        }

        // Check throttle for Open state only
        if (!forcePublish && match.State == MatchState.Open)
        {
            var lastPublish = await _blobWriter.GetLastPublishTimeAsync(matchId);
            if (lastPublish.HasValue)
            {
                var elapsed = DateTime.UtcNow - lastPublish.Value;
                if (elapsed < _throttleInterval)
                {
                    return; // Skip publish
                }
            }
        }

        var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Build blob state
        var blobState = new BlobMatchState
        {
            MatchId = matchId,
            TournamentId = match.TournamentId,
            Version = version,
            State = match.State.ToString(),
            UpdatedAt = DateTime.UtcNow,
            TableSize = 10,
            TotalPredictions = 0
        };

        // Load vote stats if match state >= Open
        if (match.State == MatchState.Open || match.State == MatchState.Locked || match.State == MatchState.Resolved)
        {
            var voteStats = await _predictionRepository.GetVoteStatsAsync(matchId);
            blobState.TotalPredictions = voteStats.TotalVotes;

            if (voteStats.TotalVotes > 0)
            {
                blobState.WinnerVotes = new WinnerVotesDto
                {
                    Town = new VoteEntry
                    {
                        Count = (int)Math.Round(voteStats.TotalVotes * voteStats.TownPercentage / 100),
                        Percent = voteStats.TownPercentage
                    },
                    Mafia = new VoteEntry
                    {
                        Count = (int)Math.Round(voteStats.TotalVotes * voteStats.MafiaPercentage / 100),
                        Percent = voteStats.MafiaPercentage
                    }
                };

                blobState.VotedOutVotes = voteStats.SlotVotes.Select(s => new SlotVoteEntry
                {
                    Slot = s.Slot,
                    Count = s.Count,
                    Percent = s.Percentage
                }).ToList();
            }
        }

        // Load match result if resolved
        if (match.State == MatchState.Resolved)
        {
            var result = await _predictionRepository.GetMatchResultAsync(matchId);
            if (result != null)
            {
                var (winningSide, correctVotedOutCsv) = result.Value;
                blobState.MatchResult = new MatchResultDto
                {
                    WinningSide = winningSide,
                    VotedOutSlots = string.IsNullOrEmpty(correctVotedOutCsv)
                        ? new List<int>()
                        : correctVotedOutCsv.Split(',').Select(int.Parse).ToList()
                };
            }
        }

        // Write to blob
        await _blobWriter.WriteStateAsync(blobState);
    }
}
