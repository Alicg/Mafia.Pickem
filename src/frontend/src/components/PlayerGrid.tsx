import React from 'react';
import './PlayerGrid.css';

interface PlayerGridProps {
  selectedSlot: number | null;
  onSelect: (slot: number) => void;
  disabled?: boolean;
}

export const PlayerGrid: React.FC<PlayerGridProps> = ({
  selectedSlot,
  onSelect,
  disabled = false
}) => {
  const slots = Array.from({ length: 10 }, (_, i) => i + 1);

  return (
    <div className="player-grid-container">
      <div className="player-grid">
        {slots.map(slot => (
          <button
            key={slot}
            className={`player-slot-btn ${selectedSlot === slot ? 'selected' : ''}`}
            onClick={() => !disabled && onSelect(slot)}
            disabled={disabled}
            type="button"
          >
            {slot}
          </button>
        ))}
      </div>
      <button
        className={`player-slot-btn no-one-btn ${selectedSlot === 0 ? 'selected' : ''}`}
        onClick={() => !disabled && onSelect(0)}
        disabled={disabled}
        type="button"
      >
        Никто
      </button>
    </div>
  );
};
