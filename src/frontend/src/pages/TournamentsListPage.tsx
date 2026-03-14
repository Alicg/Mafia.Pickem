import React, { useEffect, useState } from 'react';
import { adminDeleteTournament, getActiveTournaments } from '../lib/api';
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
  const [editingTournament, setEditingTournament] = useState<TournamentDto | null>(null);
  const [deletingTournamentId, setDeletingTournamentId] = useState<number | null>(null);

  const loadTournaments = async () => {
    try {
      const data = await getActiveTournaments();
      setTournaments(data);
      setError(null);
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

  const handleDelete = async (tournament: TournamentDto) => {
    const confirmed = window.confirm(`Удалить турнир "${tournament.name}"? Это действие необратимо.`);
    if (!confirmed) {
      return;
    }

    try {
      setDeletingTournamentId(tournament.id);
      setError(null);
      await adminDeleteTournament(tournament.id);
      setTournaments(prev => prev.filter(item => item.id !== tournament.id));
      hapticFeedback('success');
    } catch (err) {
      console.error('Failed to delete tournament:', err);
      setError(err instanceof Error ? err.message : 'Не удалось удалить турнир');
      hapticFeedback('error');
    } finally {
      setDeletingTournamentId(null);
    }
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
              <div key={t.id} className="tournament-card">
                <button className="tournament-card-main" onClick={() => handleSelect(t)}>
                  <div className="tournament-card-name">{t.name}</div>
                  {t.description && (
                    <div className="tournament-card-desc">{t.description}</div>
                  )}
                  <span className="tournament-card-arrow">›</span>
                </button>
                {isAdmin && (
                  <div className="tournament-card-actions">
                    <button
                      className="btn btn-secondary"
                      onClick={() => {
                        hapticFeedback('selection');
                        setEditingTournament(t);
                      }}
                      disabled={deletingTournamentId === t.id}
                    >
                      Редактировать
                    </button>
                    <button
                      className="btn btn-danger"
                      onClick={() => handleDelete(t)}
                      disabled={deletingTournamentId === t.id}
                    >
                      {deletingTournamentId === t.id ? 'Удаление...' : 'Удалить турнир'}
                    </button>
                  </div>
                )}
              </div>
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

      {editingTournament && (
        <CreateTournamentForm
          tournament={editingTournament}
          onSuccess={() => {
            setEditingTournament(null);
            loadTournaments();
          }}
          onCancel={() => setEditingTournament(null)}
        />
      )}
    </div>
  );
};
