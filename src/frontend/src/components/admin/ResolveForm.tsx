import React, { useEffect, useState } from 'react';
import { adminResolveMatch, getMatchStateBlob } from '../../lib/api';
import { MatchState, ResolveMatchRequest } from '../../types';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface ResolveFormProps {
  matchId: number;
  currentState: MatchState;
  onSuccess: () => void;
  onCancel: () => void;
}

export const ResolveForm: React.FC<ResolveFormProps> = ({ matchId, currentState, onSuccess, onCancel }) => {
  const [winningSide, setWinningSide] = useState<number>(0); // 0 = Town, 1 = Mafia
  const [votedOutSlots, setVotedOutSlots] = useState<number[]>([]);
  const [isInitializing, setIsInitializing] = useState(currentState === MatchState.FirstVoted);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isActive = true;

    const loadInitialResult = async () => {
      if (currentState !== MatchState.FirstVoted) {
        setIsInitializing(false);
        return;
      }

      setIsInitializing(true);
      setError(null);

      try {
        const blobState = await getMatchStateBlob(matchId);
        const initialVotedOutSlots = blobState?.matchResult?.votedOutSlots;

        if (!isActive) {
          return;
        }

        if (!initialVotedOutSlots || initialVotedOutSlots.length === 0) {
          return;
        }

        setVotedOutSlots(initialVotedOutSlots);
      } catch (err) {
        if (isActive) {
          console.error('Failed to load initial resolve data:', err);
          setError('Не удалось загрузить заголосованного игрока. Выберите его вручную.');
        }
      } finally {
        if (isActive) {
          setIsInitializing(false);
        }
      }
    };

    loadInitialResult();

    return () => {
      isActive = false;
    };
  }, [currentState, matchId]);

  const isBusy = isInitializing || isLoading;

  const toggleSlot = (slot: number) => {
    if (isBusy) {
      return;
    }

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
    if (isBusy) {
      return;
    }

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

        {isInitializing ? (
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '220px', gap: '12px' }}>
            <div className="spinner"></div>
            <div style={{ color: 'var(--tg-theme-hint-color)', textAlign: 'center' }}>
              Загружаем выбранного заголосованного игрока...
            </div>
          </div>
        ) : (
          <>
        
        {error && <div className="error-message" style={{color: 'red', marginBottom: '10px'}}>{error}</div>}
        
        <div className="form-group">
          <label className="form-label">Победившая команда</label>
          <div className="side-toggle">
            <div 
              className={`side-option ${winningSide === 0 ? 'active' : ''}`}
              onClick={() => { if (!isBusy) { setWinningSide(0); hapticFeedback('selection'); } }}
            >
              Мирные
            </div>
            <div 
              className={`side-option ${winningSide === 1 ? 'active' : ''}`}
              onClick={() => { if (!isBusy) { setWinningSide(1); hapticFeedback('selection'); } }}
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
            disabled={isBusy}
          >
            Отмена
          </button>
          <button 
            type="button" 
            className="btn btn-primary"
            onClick={handleResolve}
            disabled={isBusy}
          >
            {isLoading && <span className="btn-spinner" />}
            {isLoading ? 'Сохранение...' : 'Завершить'}
          </button>
        </div>
          </>
        )}
      </div>
    </div>
  );
};
