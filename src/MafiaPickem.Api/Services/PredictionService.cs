using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Enums;

namespace MafiaPickem.Api.Services;

public class PredictionService : IPredictionService
{
    private readonly IPredictionRepository _predictionRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITournamentParticipantRepository _tournamentParticipantRepository;
    private readonly IUserContext _userContext;

    public PredictionService(
        IPredictionRepository predictionRepository,
        IMatchRepository matchRepository,
        ITournamentRepository tournamentRepository,
        ITournamentParticipantRepository tournamentParticipantRepository,
        IUserContext userContext)
    {
        _predictionRepository = predictionRepository;
        _matchRepository = matchRepository;
        _tournamentRepository = tournamentRepository;
        _tournamentParticipantRepository = tournamentParticipantRepository;
        _userContext = userContext;
    }

    public async Task SubmitPredictionAsync(int matchId, int userId, byte predictedWinner, byte predictedVotedOut)
    {
        // Validate user is registered
        if (!_userContext.IsRegistered)
        {
            throw new InvalidOperationException("User must be registered to submit predictions");
        }

        // Validate match exists and is open
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
        {
            throw new InvalidOperationException($"Match with ID {matchId} not found");
        }

        if (match.State != MatchState.Open)
        {
            throw new InvalidOperationException("Match is not open for predictions");
        }

        var tournament = await _tournamentRepository.GetByIdAsync(match.TournamentId)
            ?? throw new InvalidOperationException($"Tournament with ID {match.TournamentId} not found");

        if (!string.IsNullOrWhiteSpace(tournament.TeamsJson))
        {
            var hasTeamSelection = await _tournamentParticipantRepository.HasTeamSelectionAsync(match.TournamentId, userId);
            if (!hasTeamSelection)
            {
                throw new InvalidOperationException("User must select a team for this tournament before submitting predictions");
            }
        }

        // Validate predicted winner
        if (predictedWinner > 1)
        {
            throw new InvalidOperationException("PredictedWinner must be 0 (Town) or 1 (Mafia)");
        }

        // Validate predicted voted out
        if (predictedVotedOut > 10)
        {
            throw new InvalidOperationException("PredictedVotedOut must be between 0 and 10");
        }

        // Submit prediction
        await _predictionRepository.UpsertAsync(matchId, userId, predictedWinner, predictedVotedOut);
    }
}
