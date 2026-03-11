import React from 'react';
import './PredictionForm.css';
import { PlayerGrid } from './PlayerGrid';
import { LAST_ROUND_LABELS, TOWN_LAST_ROUNDS, MAFIA_LAST_ROUNDS } from '../types';

interface PredictionFormProps {
  selectedWinner: number | null; // 0 = Town, 1 = Mafia
  selectedVotedOut: number | null; // 0 = None, 1-10 = Player
  selectedLastRound: number | null; // LastRound enum value
  onWinnerChange: (team: number) => void;
  onVotedOutChange: (slot: number) => void;
  onLastRoundChange: (lastRound: number) => void;
  disabled?: boolean;
}

export const PredictionForm: React.FC<PredictionFormProps> = ({
  selectedWinner,
  selectedVotedOut,
  selectedLastRound,
  onWinnerChange,
  onVotedOutChange,
  onLastRoundChange,
  disabled = false
}) => {

  const handleWinnerSelect = (team: number) => {
    if (!disabled) {
      onWinnerChange(team);
    }
  };

  const handleVotedOutSelect = (slot: number) => {
    if (!disabled) {
      onVotedOutChange(slot);
    }
  };

  const handleLastRoundSelect = (lr: number) => {
    if (!disabled) {
      onLastRoundChange(lr);
    }
  };

  const lastRoundOptions = selectedWinner === 0 ? TOWN_LAST_ROUNDS : selectedWinner === 1 ? MAFIA_LAST_ROUNDS : [];

  return (
    <div className="prediction-form">
      <div className="form-section">
        <h3>Кто победит?</h3>
        <div className="winner-toggle">
          <button
            type="button"
            className={`winner-btn town ${selectedWinner === 0 ? 'selected' : ''}`}
            onClick={() => handleWinnerSelect(0)}
            disabled={disabled}
          >
            Мирные
          </button>
          <button
            type="button"
            className={`winner-btn mafia ${selectedWinner === 1 ? 'selected' : ''}`}
            onClick={() => handleWinnerSelect(1)}
            disabled={disabled}
          >
            Мафия
          </button>
        </div>
      </div>

      <div className="form-section">
        <h3>Последний круг</h3>
        <div className={`last-round-section ${selectedWinner === null ? 'section-disabled' : ''}`}>
          <div className="last-round-toggle">
            {lastRoundOptions.map(lr => (
              <button
                key={lr}
                type="button"
                className={`last-round-btn ${selectedLastRound === lr ? 'selected' : ''}`}
                onClick={() => handleLastRoundSelect(lr)}
                disabled={disabled || selectedWinner === null}
              >
                {LAST_ROUND_LABELS[lr]}
              </button>
            ))}
            {selectedWinner === null && (
              <span className="last-round-hint">Сначала выберите победителя</span>
            )}
          </div>
        </div>
      </div>

      <div className="form-section">
        <h3>Кого заголосуют первым?</h3>
        <PlayerGrid
          selectedSlot={selectedVotedOut}
          onSelect={handleVotedOutSelect}
          disabled={disabled}
        />
      </div>
    </div>
  );
};
