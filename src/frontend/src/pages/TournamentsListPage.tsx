import React, { useEffect, useState } from 'react';
import { getActiveTournaments } from '../lib/api';
import { TournamentDto } from '../types';
import { hapticFeedback } from '../lib/telegram';
import { CreateTournamentForm } from '../components/admin/CreateTournamentForm';
import './TournamentsListPage.css';

interface TournamentsListPageProps {
  onSelect: (tournament: TournamentDto) => void;
  isAdmin?: boolean;
}

export const TournamentsListPage: React.FC<TournamentsListPageProps> = ({ onSelect, isAdmin }) => {
  const [tournaments, setTournaments] = useState<TournamentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);

  const loadTournaments = async () => {
    try {
      const data = await getActiveTournaments();
      setTournaments(data);
    } catch (err) {
      console.error('Failed to load tournaments:', err);
      setError('Не удалось загрузить турниры');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadTournaments();
  }, []);

  const handleSelect = (t: TournamentDto) => {
    hapticFeedback('selection');
    onSelect(t);
  };

  if (isLoading) {
    return (
      <div className="tournaments-list-page">
        <div className="loading-container"><div className="spinner"></div></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="tournaments-list-page">
        <div className="center-container">
          <p style={{ color: 'var(--tg-theme-hint-color)' }}>{error}</p>
          <button className="button-primary" style={{ marginTop: '16px' }} onClick={() => window.location.reload()}>
            Обновить
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="tournaments-list-page">
      <header className="tournaments-header">
        <h1>Турниры</h1>
        <div className="tournaments-header-sub">Делай прогнозы — зарабатывай очки</div>
        {isAdmin && (
          <button
            className="btn btn-primary"
            style={{ marginTop: '12px' }}
            onClick={() => { hapticFeedback('selection'); setShowCreateForm(true); }}
          >
            + Новый турнир
          </button>
        )}
      </header>

      <div className="tournaments-content">
        {tournaments.length === 0 ? (
          <div className="empty-state">Нет доступных турниров</div>
        ) : (
          <div className="tournaments-grid">
            {tournaments.map(t => (
              <button key={t.id} className="tournament-card" onClick={() => handleSelect(t)}>
                <div className="tournament-card-name">{t.name}</div>
                {t.description && (
                  <div className="tournament-card-desc">{t.description}</div>
                )}
                <span className="tournament-card-arrow">›</span>
              </button>
            ))}
          </div>
        )}
      </div>

      {showCreateForm && (
        <CreateTournamentForm
          onSuccess={() => {
            setShowCreateForm(false);
            loadTournaments();
          }}
          onCancel={() => setShowCreateForm(false)}
        />
      )}
    </div>
  );
};
