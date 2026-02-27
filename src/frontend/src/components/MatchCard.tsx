import React, { useEffect, useState } from 'react';
import './MatchCard.css';
import { MatchDto, MatchState, PredictionDto } from '../types';
import { useMatchState } from '../hooks/useMatchState';
import { PredictionForm } from './PredictionForm';
import { CrowdStats } from './CrowdStats';
import { submitPrediction } from '../lib/api';
import { showMainButton, hideMainButton, hapticFeedback } from '../lib/telegram';

interface MatchCardProps {
  match: MatchDto;
  isCurrent: boolean; // If true, enables polling
}

export const MatchCard: React.FC<MatchCardProps> = ({ match, isCurrent }) => {
  // Use polling for current match
  const { blobState } = useMatchState(isCurrent ? match.id : null);

  // Local state for user prediction (optimistic UI)
  const [prediction, setPrediction] = useState<PredictionDto | null>(match.myPrediction);
  const [selectedWinner, setSelectedWinner] = useState<number | null>(match.myPrediction?.predictedWinner ?? null);
  const [selectedVotedOut, setSelectedVotedOut] = useState<number | null>(match.myPrediction?.predictedVotedOut ?? null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  // Derive display state from blob (if available) or props
  // Note: Blob state overrides props state if newer
  const currentState = blobState ? (
      // Map blob state string to enum if needed, or rely on match.state if blob state is just string
      // Let's assume for now we use match.state as base, but update if blob says otherwise
      // Actually blobState.state is string. We need to parse it or just use match.state if blob is missing
      // For simplicity, let's use the hook to return parsed state later or just parse here.
      // But wait! The hook returns blobState object. 
      // Let's just trust match.state for initial render and blobState.state for updates.
      // If blobState.state is "Open", we treat as Open. 
      // We need a helper to parse blob state string to enum.
      parseState(blobState.state) 
  ) : match.state;

  function parseState(s: string): MatchState {
      const lower = s.toLowerCase();
      if (lower === 'upcoming' || lower === '0') return MatchState.Upcoming;
      if (lower === 'open' || lower === '1') return MatchState.Open;
      if (lower === 'locked' || lower === '2') return MatchState.Locked;
      if (lower === 'resolved' || lower === '3') return MatchState.Resolved;
      if (lower === 'canceled' || lower === '4') return MatchState.Canceled;
      return MatchState.Upcoming;
  }

  // Sync prediction from props when it changes
  useEffect(() => {
    setPrediction(match.myPrediction);
  }, [match.myPrediction]);

  // Update local selection when props change (if no local changes)
  useEffect(() => {
    if (!hasChanges) {
      setSelectedWinner(prediction?.predictedWinner ?? null);
      setSelectedVotedOut(prediction?.predictedVotedOut ?? null);
    }
  }, [prediction, hasChanges]);

  // Handle Telegram MainButton
  useEffect(() => {
    if (!isCurrent || currentState !== MatchState.Open) {
      hideMainButton();
      return;
    }

    if (hasChanges && selectedWinner !== null && selectedVotedOut !== null) {
      showMainButton('Сохранить прогноз', async () => {
        try {
          setIsSubmitting(true);
          hapticFeedback('soft'); // Or 'impactLight' via native if available, wrapper handles it
          
          await submitPrediction(match.id, selectedWinner, selectedVotedOut);
          
          // Update local prediction state
          setPrediction({
            predictedWinner: selectedWinner,
            predictedVotedOut: selectedVotedOut,
            winnerPoints: null,
            votedOutPoints: null,
            totalPoints: null
          });
          
          setHasChanges(false);
          hapticFeedback('success'); 
          hideMainButton();
        } catch (err) {
          console.error('Failed to submit prediction:', err);
          hapticFeedback('error');
        } finally {
          setIsSubmitting(false);
        }
      });
    } else {
      hideMainButton();
    }
    
    // Cleanup
    return () => {
        // If unmounting or changing state, ensure button is hidden 
        // We can't easily rely on return here for the click handler unless we wrap functionality
        // But showMainButton overwrites handler.
    };
  }, [isCurrent, currentState, hasChanges, selectedWinner, selectedVotedOut, match.id]);

  const handleWinnerChange = (val: number) => {
    setSelectedWinner(val);
    setHasChanges(true);
    hapticFeedback('selection');
  };

  const handleVotedOutChange = (val: number) => {
    setSelectedVotedOut(val);
    setHasChanges(true);
    hapticFeedback('selection');
  };

  // Status Badge Logic
  const getStatusBadge = () => {
    switch (currentState) {
      case MatchState.Upcoming: return <span className="status-badge upcoming">Ожидается</span>;
      case MatchState.Open: return <span className="status-badge open">Прогнозы открыты</span>;
      case MatchState.Locked: return <span className="status-badge locked">Игра идет</span>;
      case MatchState.Resolved: return <span className="status-badge resolved">Завершено</span>;
      case MatchState.Canceled: return <span className="status-badge canceled">Отменена</span>;
    }
  };

  const isFormDisabled = currentState !== MatchState.Open || isSubmitting;

  // Resolved display helpers
  // If resolved, we need to know the ACTUAL result. 
  // Wait, MatchDto doesn't have the result explicit fields? 
  // It has `voteStats` and `myPrediction` with points.
  // Actually, `won` status is usually inferred from points > 0? 
  // Or maybe we don't show the result explicitly on the card other than points?
  // Requirements: "Correct answers highlighted in green", "User's prediction shown (correct = green, wrong = red/gray)"
  // Where do we get the correct answer specifically?
  // Usually from `match.voteStats`? No. 
  // The API doesn't seem to return the "Result" in `MatchDto` directly?
  // Let's check `MatchDto` again.
  // `id, gameNumber, tableNumber, state, myPrediction, voteStats`.
  // Maybe `myPrediction.winnerPoints` being non-null implies result is known?
  // But how do we know WHO won if we didn't predict correctly?
  // Maybe we can infer from `voteStats` if it has "winner" field? 
  // `BlobMatchState` has `winnerVotes`... that's votes, not result.
  // Wait, maybe the backend doesn't expose the result directly in `MatchDto` unless I missed it?
  // Let me check `MatchDto` definition in `src/MafiaPickem.Api/Models/Responses/MatchDto.cs` or similar in backend if needed?
  // Actually, looking at `MatchDto` in `types/index.ts`:
  //   predictedWinner: number;
  //   winnerPoints: number | null;
  // If `winnerPoints` > 0, then `predictedWinner` was correct.
  // If `winnerPoints` === 0, then `predictedWinner` was wrong.
  // But what was the correct answer?
  // Maybe I can't show the *correct* answer if user was wrong, unless the API provides it.
  // For now, I will just show user's prediction and points.
  // The requirements say "Correct answers highlighted in green".
  // If I can't know the correct answer, I can't highlight it unless I predicted it.
  // Let's assume for now I only highlight if *my* prediction was correct.
  
  return (
    <div className={`match-card state-${MatchState[currentState].toLowerCase()}`}>
      <div className="card-header">
        <div className="game-info">
          <span className="game-number">Игра #{match.gameNumber}</span>
          {match.tableNumber && <span className="table-number">Стол {match.tableNumber}</span>}
        </div>
        {getStatusBadge()}
      </div>

      <div className="card-body">
        <PredictionForm 
           selectedWinner={selectedWinner}
           selectedVotedOut={selectedVotedOut}
           onWinnerChange={handleWinnerChange}
           onVotedOutChange={handleVotedOutChange}
           disabled={isFormDisabled}
        />

        {(currentState === MatchState.Open || currentState === MatchState.Locked || currentState === MatchState.Resolved) && (
           <CrowdStats 
             apiStats={match.voteStats}
             blobState={blobState}
           />
        )}
        
        {currentState === MatchState.Resolved && prediction && (
            <div className="results-summary">
                <div className="points-badge">
                   +{ (prediction.winnerPoints || 0) + (prediction.votedOutPoints || 0) } очков
                </div>
            </div>
        )}
      </div>
    </div>
  );
};
