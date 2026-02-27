import React from 'react';
import './PredictionForm.css';
import { PlayerGrid } from './PlayerGrid';

interface PredictionFormProps {
  selectedWinner: number | null; // 0 = Town, 1 = Mafia
  selectedVotedOut: number | null; // 0 = None, 1-10 = Player
  onWinnerChange: (team: number) => void;
  onVotedOutChange: (slot: number) => void;
  disabled?: boolean;
}

export const PredictionForm: React.FC<PredictionFormProps> = ({
  selectedWinner,
  selectedVotedOut,
  onWinnerChange,
  onVotedOutChange,
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
        <h3>Кого выгонят первым?</h3>
        <PlayerGrid
          selectedSlot={selectedVotedOut}
          onSelect={handleVotedOutSelect}
          disabled={disabled}
        />
      </div>
    </div>
  );
};
