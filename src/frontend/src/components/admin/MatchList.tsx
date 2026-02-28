import React from 'react';
import { MatchDto, MatchState } from '../../types';
import { MatchStateControls } from './MatchStateControls';
import './admin.css';

interface MatchListProps {
  matches: MatchDto[];
  onRefresh: () => void;
  onResolve: (matchId: number) => void;
}

export const MatchList: React.FC<MatchListProps> = ({ matches, onRefresh, onResolve }) => {
  if (!matches || matches.length === 0) {
    return <div className="no-matches">Нет активных игр</div>;
  }

  // Sort by game number descending (newest first)
  const sortedMatches = [...matches].sort((a, b) => a.gameNumber - b.gameNumber);

  const getStateBadgeClass = (state: MatchState) => {
    switch (state) {
      case MatchState.Upcoming: return 'badge-upcoming';
      case MatchState.Open: return 'badge-open';
      case MatchState.Locked: return 'badge-locked';
      case MatchState.Resolved: return 'badge-resolved';
      case MatchState.Canceled: return 'badge-canceled';
      default: return '';
    }
  };

  const getStateLabel = (state: MatchState) => {
    switch (state) {
      case MatchState.Upcoming: return 'Ожидание';
      case MatchState.Open: return 'Открыта';
      case MatchState.Locked: return 'Заблокирована';
      case MatchState.Resolved: return 'Завершена';
      case MatchState.Canceled: return 'Отменена';
      default: return 'Неизвестно';
    }
  };

  return (
    <div className="match-list">
      {sortedMatches.map(match => (
        <div key={match.id} className="match-card">
          <div className="match-header">
            <div className="match-info">
              Игра #{match.gameNumber}
              {match.tableNumber && <span className="match-subinfo">Стол {match.tableNumber}</span>}
            </div>
            <div className={`state-badge ${getStateBadgeClass(match.state)}`}>
              {getStateLabel(match.state)}
            </div>
          </div>
          
          <MatchStateControls 
            match={match} 
            onRefresh={onRefresh}
            onResolve={() => onResolve(match.id)} // Open modal in parent
          />
        </div>
      ))}
    </div>
  );
};
