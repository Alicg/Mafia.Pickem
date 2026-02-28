import React, { useState } from 'react';
import { adminResolveMatch } from '../../lib/api';
import { ResolveMatchRequest } from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface ResolveFormProps {
  matchId: number;
  onSuccess: () => void;
  onCancel: () => void;
}

export const ResolveForm: React.FC<ResolveFormProps> = ({ matchId, onSuccess, onCancel }) => {
  const [winningSide, setWinningSide] = useState<number>(0); // 0 = Town, 1 = Mafia
  const [votedOutSlots, setVotedOutSlots] = useState<number[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const toggleSlot = (slot: number) => {
    hapticFeedback('selection');
    if (slot === 0) {
      // Toggle "Nobody"
      if (votedOutSlots.includes(0)) {
        setVotedOutSlots([]);
      } else {
        setVotedOutSlots([0]);
      }
    } else {
      // Toggle player slot
      let newSlots = [...votedOutSlots];
      
      // If "Nobody" was selected, remove it
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

  const handleResolve = async () => {
    setIsLoading(true);
    setError(null);
    hapticFeedback();

    try {
      const request: ResolveMatchRequest = {
        winningSide,
        votedOutSlots: votedOutSlots.length > 0 ? votedOutSlots : [] // If empty, backend might complain? Or maybe empty is allowed. Let's assume empty means no one voted out if not 0.
      };
      // Actually backend likely expects [0] for nobody. If empty, maybe validation error.
      // Let's enforce selection
      if (votedOutSlots.length === 0) {
        throw new Error('Выберите "Никто" или слоты выбывших игроков');
      }

      await adminResolveMatch(matchId, request);
      hapticFeedback('success');
      onSuccess();
    } catch (err: any) {
      setError(err.message || 'Ошибка при завершении игры');
      hapticFeedback('error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2 className="modal-title">Завершение игры #{matchId}</h2>
        
        {error && <div className="error-message" style={{color: 'red', marginBottom: '10px'}}>{error}</div>}
        
        <div className="form-group">
          <label className="form-label">Победившая команда</label>
          <div className="side-toggle">
            <div 
              className={`side-option ${winningSide === 0 ? 'active' : ''}`}
              onClick={() => { setWinningSide(0); hapticFeedback('selection'); }}
            >
              Мирные
            </div>
            <div 
              className={`side-option ${winningSide === 1 ? 'active' : ''}`}
              onClick={() => { setWinningSide(1); hapticFeedback('selection'); }}
            >
              Мафия
            </div>
          </div>
        </div>
        
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
            onClick={handleResolve}
            disabled={isLoading}
          >
            {isLoading && <span className="btn-spinner" />}
            {isLoading ? 'Сохранение...' : 'Завершить'}
          </button>
        </div>
      </div>
    </div>
  );
};
