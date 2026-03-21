import React, { useEffect, useMemo, useState } from 'react';
import { createPortal } from 'react-dom';
import { LeaderboardResponse, LeaderboardEntryDto } from '../types';
import { getLeaderboard } from '../lib/api';
import { formatLeaderboardPoints } from '../lib/leaderboard';
import './LeaderboardTab.css';

interface LeaderboardTabProps {
  tournamentId: number;
  currentUserId: number;
}

type LeaderboardMode = 'players' | 'teams';

interface TeamLeaderboardEntry {
  rank: number;
  teamName: string;
  totalPoints: number;
  participantsCount: number;
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
        <div className="lb-player-meta">
          <span className="lb-name">{entry.displayName}</span>
          {entry.teamName && <span className="lb-team-name">{entry.teamName}</span>}
        </div>
      </div>
      <span className="lb-points">{formatLeaderboardPoints(entry.totalPoints)}</span>
    </div>
  );
};

const TeamLeaderboardRow: React.FC<{ entry: TeamLeaderboardEntry }> = ({ entry }) => {
  let rankDisplay: React.ReactNode = entry.rank;
  if (entry.rank === 1) rankDisplay = '🥇';
  if (entry.rank === 2) rankDisplay = '🥈';
  if (entry.rank === 3) rankDisplay = '🥉';

  const rankClass = entry.rank <= 3 ? `rank-${entry.rank}` : '';

  return (
    <div className={`lb-row ${rankClass}`}>
      <span className="lb-rank">{rankDisplay}</span>
      <div className="lb-player lb-team-player">
        <div className="lb-avatar lb-team-avatar">{entry.teamName[0]}</div>
        <div className="lb-player-meta">
          <span className="lb-name">{entry.teamName}</span>
          <span className="lb-team-name">Участников: {entry.participantsCount}</span>
        </div>
      </div>
      <span className="lb-points">{formatLeaderboardPoints(entry.totalPoints)}</span>
    </div>
  );
};

export const LeaderboardTab: React.FC<LeaderboardTabProps> = ({ tournamentId, currentUserId }) => {
  const [data, setData] = useState<LeaderboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mode, setMode] = useState<LeaderboardMode>('players');
  const [isHelpOpen, setIsHelpOpen] = useState(false);

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
    if (!isHelpOpen) {
      return undefined;
    }

    const previousOverflow = document.body.style.overflow;
    const previousTouchAction = document.body.style.touchAction;
    document.body.style.overflow = 'hidden';
    document.body.style.touchAction = 'none';

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsHelpOpen(false);
      }
    };

    window.addEventListener('keydown', handleKeyDown);

    return () => {
      document.body.style.overflow = previousOverflow;
      document.body.style.touchAction = previousTouchAction;
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isHelpOpen]);

  const entries = data?.entries ?? [];
  const currentUser = entries.find(entry => entry.userId === currentUserId) ?? null;

  const teamEntries = useMemo<TeamLeaderboardEntry[]>(() => {
    const aggregates = new Map<string, { totalPoints: number; participantsCount: number }>();

    for (const entry of entries) {
      if (!entry.teamName) {
        continue;
      }

      const existing = aggregates.get(entry.teamName) ?? { totalPoints: 0, participantsCount: 0 };
      aggregates.set(entry.teamName, {
        totalPoints: existing.totalPoints + entry.totalPoints,
        participantsCount: existing.participantsCount + 1,
      });
    }

    return Array.from(aggregates.entries())
      .map(([teamName, value]) => ({
        teamName,
        totalPoints: value.totalPoints,
        participantsCount: value.participantsCount,
      }))
      .sort((left, right) => right.totalPoints - left.totalPoints || left.teamName.localeCompare(right.teamName))
      .map((entry, index) => ({ ...entry, rank: index + 1 }));
  }, [entries]);

  const helpOverlay = isHelpOpen && typeof document !== 'undefined'
    ? createPortal(
        <div className="lb-help-overlay" role="dialog" aria-modal="true" aria-labelledby="lb-help-title" onClick={() => setIsHelpOpen(false)}>
          <div className="lb-help-modal" onClick={(event) => event.stopPropagation()}>
            <div className="lb-help-header">
              <div>
                <h3 id="lb-help-title" className="lb-help-title">Как считаются очки</h3>
                <p className="lb-help-subtitle">Коротко, с числами и примерами</p>
              </div>
              <button type="button" className="lb-help-close" aria-label="Закрыть подсказку" onClick={() => setIsHelpOpen(false)}>
                ×
              </button>
            </div>

            <div className="lb-help-body">
              <div className="lb-help-block">
                <ul className="lb-help-list">
                  <li>Очки начисляются за правильные прогнозы.</li>
                  <li>Базовое количество очков за правильный прогноз — 15.</li>
                  <li>Чем меньше людей угадали тот же ответ, тем больше бонус за редкость.</li>
                  <li>Все типы прогнозов оцениваются по одной и той же формуле.</li>
                </ul>
              </div>

              <div className="lb-help-block lb-help-formula">
                <p className="lb-help-label">Простая логика</p>
                <ul className="lb-help-list">
                  <li>Очки = 15 x бонус за редкость</li>
                  <li>Если ответ угадали многие, бонус ближе к x1.</li>
                  <li>Если ответ угадали немногие, бонус растет, но максимум до x3.</li>
                </ul>
              </div>

              <div className="lb-help-block">
                <p className="lb-help-label">Что это значит на практике</p>
                <ul className="lb-help-list">
                  <li>Предположим в Угадайке участвуют 100 игроков.</li>
                  <li>Если правильный ответ угадали почти все 100 игроков, ты получишь около 15 очков.</li>
                  <li>Если угадали 50 игроков, это около 20 очков.</li>
                  <li>Если угадали 10 игроков, это около 32 очков.</li>
                  <li>Если угадали 1-2 игрока, это уже почти максимум: 45 очков.</li>
                </ul>
              </div>
            </div>

            <div className="lb-help-footer">
              <button type="button" className="lb-help-dismiss" onClick={() => setIsHelpOpen(false)}>
                Понятно
              </button>
            </div>
          </div>
        </div>,
        document.body,
      )
    : null;

  if (loading) {
    return <div className="lb-loading"><div className="spinner"></div></div>;
  }

  if (error || !data) {
    return <div className="lb-error">{error || 'Нет данных'}</div>;
  }

  return (
    <>
      <div className="leaderboard-tab">
        <div className="lb-topbar">
          <div>
            <div className="lb-title">Лидерборд</div>
            <div className="lb-subtitle">Очки зависят от точности прогноза и того, насколько редким оказался верный ответ.</div>
          </div>
          <button type="button" className="lb-help-btn" onClick={() => setIsHelpOpen(true)}>
            Как считаются очки?
          </button>
        </div>

        {teamEntries.length > 0 && (
          <div className="lb-mode-switcher">
            <button className={`lb-mode-btn ${mode === 'players' ? 'active' : ''}`} onClick={() => setMode('players')}>
              Игроки
            </button>
            <button className={`lb-mode-btn ${mode === 'teams' ? 'active' : ''}`} onClick={() => setMode('teams')}>
              Команды
            </button>
          </div>
        )}

        <div className="lb-header-row">
          <span className="lb-rank">#</span>
          <span className="lb-player">{mode === 'players' ? 'Игрок' : 'Команда'}</span>
          <span className="lb-points">Очки</span>
        </div>

        {mode === 'players' && entries.map(entry => (
          <LeaderboardRow key={entry.rank} entry={entry} isCurrentUser={entry.userId === currentUserId} />
        ))}

        {mode === 'teams' && teamEntries.map(entry => (
          <TeamLeaderboardRow key={entry.teamName} entry={entry} />
        ))}

        {mode === 'players' && entries.length === 0 && (
          <div className="lb-empty">Пока нет результатов</div>
        )}

        {mode === 'teams' && teamEntries.length === 0 && (
          <div className="lb-empty">Командный рейтинг появится после выбора команд</div>
        )}

        {mode === 'players' && currentUser && !entries.some(entry => entry.userId === currentUserId) && (
          <div className="lb-current-user-sticky">
            <LeaderboardRow entry={currentUser} isCurrentUser />
          </div>
        )}
      </div>

      {helpOverlay}
    </>
  );
};
