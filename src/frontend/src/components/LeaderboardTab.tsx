import React, { useEffect, useState } from 'react';
import { LeaderboardResponse, LeaderboardEntryDto } from '../types';
import { getLeaderboard } from '../lib/api';
import './LeaderboardTab.css';

interface LeaderboardTabProps {
  tournamentId: number;
  currentUserId: number;
}

const LeaderboardRow: React.FC<{ entry: LeaderboardEntryDto; isCurrentUser: boolean }> = ({ entry, isCurrentUser }) => {
  let rankDisplay: React.ReactNode = entry.rank;
  if (entry.rank === 1) rankDisplay = '🥇';
  if (entry.rank === 2) rankDisplay = '🥈';
  if (entry.rank === 3) rankDisplay = '🥉';

  const rankClass = entry.rank <= 3 ? `rank-${entry.rank}` : '';

  return (
    <div className={`lb-row ${isCurrentUser ? 'current-user' : ''} ${rankClass}`}>
      <span className="lb-rank">{rankDisplay}</span>
      <div className="lb-player">
        <div className="lb-avatar">
          {entry.photoUrl ? (
            <img src={entry.photoUrl} alt="" className="lb-avatar-img" />
          ) : (
            entry.displayName[0]
          )}
        </div>
        <span className="lb-name">{entry.displayName}</span>
      </div>
      <span className="lb-points">{entry.totalPoints}</span>
    </div>
  );
};

export const LeaderboardTab: React.FC<LeaderboardTabProps> = ({ tournamentId, currentUserId }) => {
  const [data, setData] = useState<LeaderboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchLeaderboard() {
      try {
        setLoading(true);
        const result = await getLeaderboard(tournamentId);
        setData(result);
      } catch (err) {
        console.error('Failed to fetch leaderboard:', err);
        setError('Не удалось загрузить таблицу лидеров');
      } finally {
        setLoading(false);
      }
    }
    fetchLeaderboard();
  }, [tournamentId]);

  if (loading) {
    return <div className="lb-loading"><div className="spinner"></div></div>;
  }

  if (error || !data) {
    return <div className="lb-error">{error || 'Нет данных'}</div>;
  }

  const currentUser = data.entries.find(e => e.userId === currentUserId) ?? null;

  return (
    <div className="leaderboard-tab">
      <div className="lb-header-row">
        <span className="lb-rank">#</span>
        <span className="lb-player">Игрок</span>
        <span className="lb-points">Очки</span>
      </div>

      {data.entries.map(entry => (
        <LeaderboardRow key={entry.rank} entry={entry} isCurrentUser={entry.userId === currentUserId} />
      ))}

      {data.entries.length === 0 && (
        <div className="lb-empty">Пока нет результатов</div>
      )}

      {currentUser && !data.entries.some(e => e.userId === currentUserId) && (
        <div className="lb-current-user-sticky">
          <LeaderboardRow entry={currentUser} isCurrentUser />
        </div>
      )}
    </div>
  );
};
