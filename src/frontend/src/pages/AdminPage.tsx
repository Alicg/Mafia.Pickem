import React, { useEffect, useState } from 'react';
import { 
  getTournamentMatches, 
  adminGetTournamentStats
} from '../lib/api';
import { MatchInfo, MatchState, TournamentStats as TournamentStatsType } from '../types';
import { MatchList } from '../components/admin/MatchList';
import { CreateMatchForm } from '../components/admin/CreateMatchForm';
import { ResolveForm } from '../components/admin/ResolveForm';
import { SetFirstVotedForm } from '../components/admin/SetFirstVotedForm';
import { hapticFeedback } from '../lib/telegram';
import '../components/admin/admin.css';

interface AdminPageProps {
  tournamentId: number;
  onBack: () => void;
}

export const AdminPage: React.FC<AdminPageProps> = ({ tournamentId, onBack }) => {
  const [matches, setMatches] = useState<MatchInfo[]>([]);
  const [stats, setStats] = useState<TournamentStatsType | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [resolvingMatch, setResolvingMatch] = useState<{ matchId: number; currentState: MatchState } | null>(null);
  const [firstVotedMatchId, setFirstVotedMatchId] = useState<number | null>(null);

  const loadData = async () => {
    setIsLoading(true);
    try {
      const [matchesData, statsData] = await Promise.all([
        getTournamentMatches(tournamentId),
        adminGetTournamentStats(tournamentId)
      ]);
      setMatches(matchesData);
      setStats(statsData);
    } catch (error) {
      console.error('Failed to load admin data:', error);
      alert('Ошибка загрузки данных');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [tournamentId]);

  const handleRefresh = () => {
    hapticFeedback();
    loadData();
  };

  const handleCreateOpen = () => {
    hapticFeedback();
    setShowCreateForm(true);
  };

  const handleCreateSuccess = () => {
    setShowCreateForm(false);
    loadData();
  };

  const handleResolveOpen = (matchId: number, currentState: MatchState) => {
    hapticFeedback();
    setResolvingMatch({ matchId, currentState });
  };

  const handleResolveSuccess = () => {
    setResolvingMatch(null);
    loadData();
  };

  const handleSetFirstVotedOpen = (matchId: number) => {
    hapticFeedback();
    setFirstVotedMatchId(matchId);
  };

  const handleSetFirstVotedSuccess = () => {
    setFirstVotedMatchId(null);
    loadData();
  };

  return (
    <div className="admin-container">
      <div className="admin-header">
        <button className="btn btn-secondary" onClick={onBack}>
          ← Назад
        </button>
        <div className="admin-title">Админ панель</div>
        <button className="btn btn-primary" onClick={handleRefresh}>
          ↻
        </button>
      </div>

      {stats && (
        <div className="stats-grid">
          <div className="stat-item">
            <div className="stat-value">{stats.totalMatches}</div>
            <div className="stat-label">Всего игр</div>
          </div>
          <div className="stat-item">
            <div className="stat-value">{stats.totalPredictions}</div>
            <div className="stat-label">Прогнозов</div>
          </div>
        </div>
      )}

      <button 
        className="btn btn-primary" 
        style={{ width: '100%', marginBottom: '20px' }}
        onClick={handleCreateOpen}
      >
        + Создать игру
      </button>

      {isLoading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '20px' }}><div className="spinner"></div></div>
      ) : (
        <MatchList 
          matches={matches} 
          onRefresh={loadData}
          onResolve={handleResolveOpen}
          onSetFirstVoted={handleSetFirstVotedOpen}
        />
      )}

      {showCreateForm && (
        <CreateMatchForm 
          tournamentId={tournamentId}
          onSuccess={handleCreateSuccess}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      {resolvingMatch !== null && (
        <ResolveForm 
          matchId={resolvingMatch.matchId}
          currentState={resolvingMatch.currentState}
          onSuccess={handleResolveSuccess}
          onCancel={() => setResolvingMatch(null)}
        />
      )}

      {firstVotedMatchId !== null && (
        <SetFirstVotedForm
          matchId={firstVotedMatchId}
          onSuccess={handleSetFirstVotedSuccess}
          onCancel={() => setFirstVotedMatchId(null)}
        />
      )}
    </div>
  );
};
