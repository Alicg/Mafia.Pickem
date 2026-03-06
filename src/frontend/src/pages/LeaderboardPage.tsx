import React, { useEffect, useState } from 'react';
import './LeaderboardPage.css';
import { LeaderboardResponse, LeaderboardEntryDto } from '../types';
import { getLeaderboard, getProfile } from '../lib/api';
import { hapticFeedback } from '../lib/telegram';

interface LeaderboardPageProps {
  tournamentId: number;
  onBack: () => void;
}

const LeaderboardRow: React.FC<{ entry: LeaderboardEntryDto; isCurrentUser: boolean }> = ({ entry, isCurrentUser }) => {
  let rankDisplay: React.ReactNode = entry.rank;
  if (entry.rank === 1) rankDisplay = '🥇';
  if (entry.rank === 2) rankDisplay = '🥈';
  if (entry.rank === 3) rankDisplay = '🥉';

  return (
    <div className={`leaderboard-row ${isCurrentUser ? 'current-user' : ''}`}>
      <span className="rank-col">{rankDisplay}</span>
      <div className="player-col">
          <div className="avatar-placeholder">
            {entry.photoUrl ? (
                <img src={entry.photoUrl} alt={entry.displayName[0]} className="avatar-img" />
            ) : (
                entry.displayName[0]
            )}
          </div>
          <div className="player-info">
             <span className="player-name">{entry.displayName}</span>
             {isCurrentUser && <span className="you-badge">(Вы)</span>}
          </div>
      </div>
      <span className="points-col">{entry.totalPoints}</span>
    </div>
  );
};

export const LeaderboardPage: React.FC<LeaderboardPageProps> = ({ tournamentId, onBack }) => {
  const [data, setData] = useState<LeaderboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentUserId, setCurrentUserId] = useState<number | null>(null);

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

  useEffect(() => {
    async function fetchCurrentUser() {
      try {
        const profile = await getProfile();
        setCurrentUserId(profile.id);
      } catch (err) {
        console.warn('Failed to fetch current user for leaderboard', err);
      }
    }

    fetchCurrentUser();
  }, []);

  const handleBack = () => {
    hapticFeedback('selection');
    onBack();
  };

  if (loading) {
    return (
      <div className="leaderboard-page loading">
        <div className="spinner"></div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="leaderboard-page error">
        <p>{error || 'Нет данных'}</p>
        <button className="back-btn" onClick={handleBack}>Назад</button>
      </div>
    );
  }

  return (
    <div className="leaderboard-page">
      <div className="leaderboard-header">
        <button className="back-icon-btn" onClick={handleBack}>
          ←
        </button>
        <h2>Таблица лидеров</h2>
      </div>
      
      <div className="leaderboard-list-container">
          <div className="leaderboard-row header">
            <span className="rank-col">#</span>
            <span className="player-col">Игрок</span>
            <span className="points-col">Очки</span>
          </div>
          
          <div className="leaderboard-scroll-area">
            {data.entries.map((entry) => (
              <LeaderboardRow key={entry.rank} entry={entry} isCurrentUser={entry.userId === currentUserId} />
            ))}
            
            {data.entries.length === 0 && (
                <div className="empty-state">Пока нет результатов</div>
            )}
          </div>
      </div>
    </div>
  );
};

