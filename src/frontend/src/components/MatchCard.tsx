import React, { useEffect, useState } from 'react';
import './MatchCard.css';
import { MatchDto, MatchState, PredictionDto } from '../types';
import { useMatchState } from '../hooks/useMatchState';
import { PredictionForm } from './PredictionForm';
import { CrowdStats } from './CrowdStats';
import { MatchStateControls } from './admin/MatchStateControls';
import { submitPrediction } from '../lib/api';
import { showMainButton, hideMainButton, hapticFeedback } from '../lib/telegram';

interface MatchCardProps {
  match: MatchDto;
  isExpanded: boolean;
  canExpand: boolean;
  onToggle: () => void;
  isAdmin: boolean;
  onRefresh: () => void;
  onResolve: () => void;
}

export const MatchCard: React.FC<MatchCardProps> = ({
  match, isExpanded, canExpand, onToggle, isAdmin, onRefresh, onResolve
}) => {
  // Use polling only for expanded match
  const { blobState } = useMatchState(isExpanded ? match.id : null);

  // Local state for user prediction (optimistic UI)
  const [prediction, setPrediction] = useState<PredictionDto | null>(match.myPrediction);
  const [selectedWinner, setSelectedWinner] = useState<number | null>(match.myPrediction?.predictedWinner ?? null);
  const [selectedVotedOut, setSelectedVotedOut] = useState<number | null>(match.myPrediction?.predictedVotedOut ?? null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  const currentState = blobState ? parseState(blobState.state) : match.state;

  function parseState(s: string): MatchState {
    const lower = s.toLowerCase();
    if (lower === 'upcoming' || lower === '0') return MatchState.Upcoming;
    if (lower === 'open' || lower === '1') return MatchState.Open;
    if (lower === 'locked' || lower === '2') return MatchState.Locked;
    if (lower === 'resolved' || lower === '3') return MatchState.Resolved;
    if (lower === 'canceled' || lower === '4') return MatchState.Canceled;
    return MatchState.Upcoming;
  }

  useEffect(() => {
    setPrediction(match.myPrediction);
  }, [match.myPrediction]);

  useEffect(() => {
    if (!hasChanges) {
      setSelectedWinner(prediction?.predictedWinner ?? null);
      setSelectedVotedOut(prediction?.predictedVotedOut ?? null);
    }
  }, [prediction, hasChanges]);

  // Handle Telegram MainButton for prediction submission
  useEffect(() => {
    if (!isExpanded || currentState !== MatchState.Open) {
      hideMainButton();
      return;
    }

    if (hasChanges && selectedWinner !== null && selectedVotedOut !== null) {
      showMainButton('Сохранить прогноз', async () => {
        try {
          setIsSubmitting(true);
          hapticFeedback('soft');
          await submitPrediction(match.id, selectedWinner, selectedVotedOut);
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
  }, [isExpanded, currentState, hasChanges, selectedWinner, selectedVotedOut, match.id]);

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

  return (
    <div className={`match-card state-${MatchState[currentState].toLowerCase()} ${isExpanded ? 'expanded' : ''}`}>
      <button
        className={`card-header ${canExpand ? 'expandable' : ''}`}
        onClick={onToggle}
        disabled={!canExpand}
      >
        <div className="game-info">
          <span className="game-number">Игра #{match.gameNumber}</span>
          {match.tableNumber && <span className="table-number">Стол {match.tableNumber}</span>}
        </div>
        <div className="header-right">
          {getStatusBadge()}
          {canExpand && (
            <span className={`expand-arrow ${isExpanded ? 'open' : ''}`}>▼</span>
          )}
        </div>
      </button>

      {isExpanded && (
        <div className="card-body">
          {/* Open: prediction form + stats */}
          {currentState === MatchState.Open && (
            <>
              <PredictionForm
                selectedWinner={selectedWinner}
                selectedVotedOut={selectedVotedOut}
                onWinnerChange={handleWinnerChange}
                onVotedOutChange={handleVotedOutChange}
                disabled={isFormDisabled}
              />
              <CrowdStats apiStats={match.voteStats} blobState={blobState} prediction={prediction} />
            </>
          )}

          {/* Locked: stats only */}
          {currentState === MatchState.Locked && (
            <CrowdStats apiStats={match.voteStats} blobState={blobState} prediction={prediction} />
          )}

          {/* Resolved: stats + results */}
          {currentState === MatchState.Resolved && (
            <>
              <CrowdStats apiStats={match.voteStats} blobState={blobState} prediction={prediction} />
              {prediction && (
                <div className="results-summary">
                  <div className="points-badge">
                    +{(prediction.winnerPoints || 0) + (prediction.votedOutPoints || 0)} очков
                  </div>
                </div>
              )}
            </>
          )}

          {/* Admin controls inline */}
          {isAdmin && (
            <MatchStateControls
              match={match}
              onRefresh={onRefresh}
              onResolve={onResolve}
            />
          )}
        </div>
      )}
    </div>
  );
};
