import React, { useState } from 'react';
import { MatchState } from '../../types';
import { 
  adminOpenMatch, 
  adminRevertToUpcoming, 
  adminLockMatch, 
  adminDeleteMatch 
} from '../../lib/api';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface MatchStateControlsProps {
  matchId: number;
  currentState: MatchState;
  onRefresh: () => void;
  onResolve: () => void; // Callback to open resolve modal
}

export const MatchStateControls: React.FC<MatchStateControlsProps> = ({ matchId, currentState, onRefresh, onResolve }) => {
  const [loadingAction, setLoadingAction] = useState<string | null>(null);

  const handleAction = async (actionKey: string, action: () => Promise<any>, confirmMessage?: string) => {
    if (confirmMessage && !window.confirm(confirmMessage)) {
      return;
    }

    setLoadingAction(actionKey);
    hapticFeedback();
    
    try {
      await action();
      hapticFeedback('success');
      onRefresh();
    } catch (error) {
      console.error('Action failed', error);
      hapticFeedback('error');
      alert('Ошибка выполнения действия');
    } finally {
      setLoadingAction(null);
    }
  };

  const isLoading = loadingAction !== null;

  if (currentState === MatchState.Resolved || currentState === MatchState.Canceled) {
    return null;
  }

  return (
    <div className="match-controls">
      {/* Upcoming -> Open */}
      {currentState === MatchState.Upcoming && (
        <button 
          className="btn btn-success"
          onClick={() => handleAction('open', () => adminOpenMatch(matchId))}
          disabled={isLoading}
        >
          {loadingAction === 'open' && <span className="btn-spinner" />}
          Открыть
        </button>
      )}

      {/* Open -> Upcoming */}
      {currentState === MatchState.Open && (
        <button 
          className="btn btn-secondary"
          onClick={() => handleAction(
            'revert',
            () => adminRevertToUpcoming(matchId),
            'Вернуть игру в статус "Ожидание"?'
          )}
          disabled={isLoading}
        >
          {loadingAction === 'revert' && <span className="btn-spinner" />}
          ← Ожидание
        </button>
      )}

      {/* Open -> Lock */}
      {currentState === MatchState.Open && (
        <button 
          className="btn btn-warning"
          onClick={() => handleAction('lock', () => adminLockMatch(matchId))}
          disabled={isLoading}
        >
          {loadingAction === 'lock' && <span className="btn-spinner" />}
          Заблокировать
        </button>
      )}

      {/* Locked -> Resolve */}
      {currentState === MatchState.Locked && (
        <button 
          className="btn btn-primary"
          onClick={onResolve}
          disabled={isLoading}
        >
          Завершить
        </button>
      )}

      {/* Delete (Available in all active states) */}
      <button 
        className="btn btn-danger"
        onClick={() => handleAction(
          'delete',
          () => adminDeleteMatch(matchId), 
          'Вы уверены, что хотите удалить эту игру? Все прогнозы и данные будут безвозвратно удалены.'
        )}
        disabled={isLoading}
      >
        {loadingAction === 'delete' && <span className="btn-spinner" />}
        Удалить
      </button>
    </div>
  );
};
