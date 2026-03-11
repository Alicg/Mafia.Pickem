import React, { useState } from 'react';
import { adminSetFirstVoted } from '../../lib/api';
import { SetFirstVotedRequest } from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface SetFirstVotedFormProps {
  matchId: number;
  onSuccess: () => void;
  onCancel: () => void;
}

export const SetFirstVotedForm: React.FC<SetFirstVotedFormProps> = ({ matchId, onSuccess, onCancel }) => {
  const [votedOutSlots, setVotedOutSlots] = useState<number[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const toggleSlot = (slot: number) => {
    hapticFeedback('selection');
    if (slot === 0) {
      if (votedOutSlots.includes(0)) {
        setVotedOutSlots([]);
      } else {
        setVotedOutSlots([0]);
      }
    } else {
      let newSlots = [...votedOutSlots];
      if (newSlots.includes(0)) {
        newSlots = newSlots.filter(s => s !== 0);
      }
      if (newSlots.includes(slot)) {
        newSlots = newSlots.filter(s => s !== slot);
      } else {
        newSlots.push(slot);
      }
      setVotedOutSlots(newSlots);
    }
  };

  const handleSubmit = async () => {
    setIsLoading(true);
    setError(null);
    hapticFeedback();

    try {
      if (votedOutSlots.length === 0) {
        throw new Error('Выберите "Никто" или слоты выбывших игроков');
      }

      const request: SetFirstVotedRequest = {
        votedOutSlots,
      };

      await adminSetFirstVoted(matchId, request);
      hapticFeedback('success');
      onSuccess();
    } catch (err: any) {
      setError(err.message || 'Ошибка при установке первого заголосованного');
      hapticFeedback('error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2 className="modal-title">Первый заголосованный — Игра #{matchId}</h2>
        
        {error && <div className="error-message" style={{color: 'red', marginBottom: '10px'}}>{error}</div>}
        
        <div className="form-group">
          <label className="form-label">Убитые голосованием</label>
          <div 
            className={`nobody-option ${votedOutSlots.includes(0) ? 'selected' : ''}`}
            onClick={() => toggleSlot(0)}
          >
            Никто (0)
          </div>
          
          <div className="player-grid">
            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map(slot => (
              <div 
                key={slot}
                className={`player-slot ${votedOutSlots.includes(slot) ? 'selected' : ''}`}
                onClick={() => toggleSlot(slot)}
              >
                {slot}
              </div>
            ))}
          </div>
        </div>
        
        <div className="form-actions">
          <button 
            type="button" 
            className="btn btn-secondary" 
            onClick={onCancel}
            disabled={isLoading}
          >
            Отмена
          </button>
          <button 
            type="button" 
            className="btn btn-primary"
            onClick={handleSubmit}
            disabled={isLoading}
          >
            {isLoading && <span className="btn-spinner" />}
            {isLoading ? 'Сохранение...' : 'Установить'}
          </button>
        </div>
      </div>
    </div>
  );
};
