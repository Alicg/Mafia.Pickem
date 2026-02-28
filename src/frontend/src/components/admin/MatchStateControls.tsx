import React, { useState } from 'react';
import { MatchDto, MatchState } from '../../types';
import { 
  adminOpenMatch, 
  adminRevertToUpcoming, 
  adminLockMatch, 
  adminCancelMatch 
} from '../../lib/api';
import { hapticFeedback } from '../../lib/telegram';
import './admin.css';

interface MatchStateControlsProps {
  match: MatchDto;
  onRefresh: () => void;
  onResolve: () => void; // Callback to open resolve modal
}

export const MatchStateControls: React.FC<MatchStateControlsProps> = ({ match, onRefresh, onResolve }) => {
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

  if (match.state === MatchState.Resolved || match.state === MatchState.Canceled) {
    return null;
  }

  return (
    <div className="match-controls">
      {/* Upcoming -> Open */}
      {match.state === MatchState.Upcoming && (
        <button 
          className="btn btn-success"
          onClick={() => handleAction('open', () => adminOpenMatch(match.id))}
          disabled={isLoading}
        >
          {loadingAction === 'open' && <span className="btn-spinner" />}
          Открыть
        </button>
      )}

      {/* Open -> Upcoming */}
      {match.state === MatchState.Open && (
        <button 
          className="btn btn-secondary"
          onClick={() => handleAction(
            'revert',
            () => adminRevertToUpcoming(match.id),
            'Вернуть игру в статус "Ожидание"?'
          )}
          disabled={isLoading}
        >
          {loadingAction === 'revert' && <span className="btn-spinner" />}
          ← Ожидание
        </button>
      )}

      {/* Open -> Lock */}
      {match.state === MatchState.Open && (
        <button 
          className="btn btn-warning"
          onClick={() => handleAction('lock', () => adminLockMatch(match.id))}
          disabled={isLoading}
        >
          {loadingAction === 'lock' && <span className="btn-spinner" />}
          Заблокировать
        </button>
      )}

      {/* Locked -> Resolve */}
      {match.state === MatchState.Locked && (
        <button 
          className="btn btn-primary"
          onClick={onResolve}
          disabled={isLoading}
        >
          Завершить
        </button>
      )}

      {/* Cancel (Available in all active states) */}
      <button 
        className="btn btn-danger"
        onClick={() => handleAction(
          'cancel',
          () => adminCancelMatch(match.id), 
          'Вы уверены, что хотите отменить эту игру? Это действие нельзя отменить.'
        )}
        disabled={isLoading}
      >
        {loadingAction === 'cancel' && <span className="btn-spinner" />}
        Отменить
      </button>
    </div>
  );
};
