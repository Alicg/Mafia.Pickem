import React, { useEffect, useState } from 'react';
import './MatchCard.css';
import { MatchInfo, MatchState, PredictionDto, BlobMatchState } from '../types';
import { PredictionForm } from './PredictionForm';
import { CrowdStats } from './CrowdStats';
import { MatchStateControls } from './admin/MatchStateControls';
import { submitPrediction, deletePrediction } from '../lib/api';
import { hapticFeedback } from '../lib/telegram';

interface MatchCardProps {
  matchInfo: MatchInfo;
  blobState: BlobMatchState | null;
  prediction: PredictionDto | null;
  isExpanded: boolean;
  canExpand: boolean;
  onToggle: () => void;
  isAdmin: boolean;
  onPredictionChange: (prediction: PredictionDto | null) => void;
  onRefresh: () => void;
  onResolve: () => void;
}

function parseState(s: string): MatchState {
  const lower = s.toLowerCase();
  if (lower === 'upcoming' || lower === '0') return MatchState.Upcoming;
  if (lower === 'open' || lower === '1') return MatchState.Open;
  if (lower === 'locked' || lower === '2') return MatchState.Locked;
  if (lower === 'resolved' || lower === '3') return MatchState.Resolved;
  if (lower === 'canceled' || lower === '4') return MatchState.Canceled;
  return MatchState.Upcoming;
}

export const MatchCard: React.FC<MatchCardProps> = ({
  matchInfo, blobState, prediction, isExpanded, canExpand, onToggle, isAdmin, onPredictionChange, onRefresh, onResolve
}) => {
  const [selectedWinner, setSelectedWinner] = useState<number | null>(prediction?.predictedWinner ?? null);
  const [selectedVotedOut, setSelectedVotedOut] = useState<number | null>(prediction?.predictedVotedOut ?? null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  const currentState = blobState ? parseState(blobState.state) : matchInfo.state;
  const isBlobLoading = blobState === null;

  useEffect(() => {
    if (!hasChanges) {
      setSelectedWinner(prediction?.predictedWinner ?? null);
      setSelectedVotedOut(prediction?.predictedVotedOut ?? null);
    }
  }, [prediction, hasChanges]);

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
  const showInlineSubmit = hasChanges && selectedWinner !== null && selectedVotedOut !== null && currentState === MatchState.Open;

  const handleInlineSubmit = async () => {
    if (selectedWinner === null || selectedVotedOut === null) return;
    try {
      setIsSubmitting(true);
      hapticFeedback('soft');
      await submitPrediction(matchInfo.id, selectedWinner, selectedVotedOut);
      const newPrediction: PredictionDto = {
        predictedWinner: selectedWinner,
        predictedVotedOut: selectedVotedOut,
        winnerPoints: null,
        votedOutPoints: null,
        totalPoints: null
      };
      onPredictionChange(newPrediction);
      setHasChanges(false);
      hapticFeedback('success');
    } catch (err) {
      console.error('Failed to submit prediction:', err);
      hapticFeedback('error');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelVote = async () => {
    try {
      setIsSubmitting(true);
      hapticFeedback('soft');
      await deletePrediction(matchInfo.id);
      onPredictionChange(null);
      setSelectedWinner(null);
      setSelectedVotedOut(null);
      setHasChanges(false);
      hapticFeedback('success');
    } catch (err) {
      console.error('Failed to cancel prediction:', err);
      hapticFeedback('error');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className={`match-card state-${MatchState[currentState].toLowerCase()} ${isExpanded ? 'expanded' : ''}`}>
      <button
        className={`card-header ${canExpand ? 'expandable' : ''}`}
        onClick={onToggle}
        disabled={!canExpand}
      >
        <div className="game-info">
          <span className="game-number">Игра #{matchInfo.gameNumber}</span>
          {matchInfo.tableNumber && <span className="table-number">Стол {matchInfo.tableNumber}</span>}
        </div>
        <div className="header-right">
          {isBlobLoading && <span className="blob-spinner" />}
          {getStatusBadge()}
          {canExpand && (
            <span className={`expand-arrow ${isExpanded ? 'open' : ''}`}>▼</span>
          )}
        </div>
      </button>

      {isExpanded && (
        <div className="card-body">
          {/* Open: show form OR stats depending on prediction */}
          {currentState === MatchState.Open && !prediction && (
            <>
              <PredictionForm
                selectedWinner={selectedWinner}
                selectedVotedOut={selectedVotedOut}
                onWinnerChange={handleWinnerChange}
                onVotedOutChange={handleVotedOutChange}
                disabled={isFormDisabled}
              />
              {showInlineSubmit && (
                <button
                  className="inline-submit-btn"
                  onClick={handleInlineSubmit}
                  disabled={isSubmitting}
                >
                  {isSubmitting && <span className="btn-spinner" />}
                  {isSubmitting ? 'Сохраняем...' : 'Сохранить прогноз'}
                </button>
              )}
            </>
          )}
          {currentState === MatchState.Open && prediction && (
            <>
              <CrowdStats blobState={blobState} prediction={prediction} />
              <button
                className="inline-submit-btn cancel-vote-btn"
                onClick={handleCancelVote}
                disabled={isSubmitting}
              >
                {isSubmitting && <span className="btn-spinner" />}
                {isSubmitting ? 'Отмена...' : 'Отменить голос'}
              </button>
            </>
          )}

          {/* Locked: stats only */}
          {currentState === MatchState.Locked && (
            <CrowdStats blobState={blobState} prediction={prediction} />
          )}

          {/* Resolved: stats + results */}
          {currentState === MatchState.Resolved && (
            <>
              <CrowdStats blobState={blobState} prediction={prediction} />
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
              matchId={matchInfo.id}
              currentState={currentState}
              onRefresh={onRefresh}
              onResolve={onResolve}
            />
          )}
        </div>
      )}
    </div>
  );
};
