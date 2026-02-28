import React, { useEffect, useState } from 'react';
import { getProfile, getTournamentMatches } from '../lib/api';
import { TournamentDto, UserProfile, MatchDto, MatchState } from '../types';
import { MatchCard } from '../components/MatchCard';
import { LeaderboardTab } from '../components/LeaderboardTab';
import { CreateMatchForm } from '../components/admin/CreateMatchForm';
import { ResolveForm } from '../components/admin/ResolveForm';
import { hapticFeedback } from '../lib/telegram';
import './TournamentPage.css';

type TabState = 'games' | 'leaders';

interface TournamentPageProps {
  tournament: TournamentDto;
  onBack: () => void;
}

export const TournamentPage: React.FC<TournamentPageProps> = ({ tournament, onBack }) => {
  const [activeTab, setActiveTab] = useState<TabState>('games');
  const [matches, setMatches] = useState<MatchDto[]>([]);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [expandedMatchId, setExpandedMatchId] = useState<number | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [resolvingMatchId, setResolvingMatchId] = useState<number | null>(null);

  const loadData = async () => {
    try {
      const [userProfile, tournamentMatches] = await Promise.all([
        getProfile(),
        getTournamentMatches(tournament.id)
      ]);
      setProfile(userProfile);
      setMatches(tournamentMatches.sort((a, b) => a.gameNumber - b.gameNumber));
    } catch (err) {
      console.error('Failed to init tournament page:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [tournament.id]);

  // Auto-expand the first expandable match (Open or Locked)
  useEffect(() => {
    if (matches.length > 0 && expandedMatchId === null) {
      const expandable = matches.find(
        m => m.state === MatchState.Open || m.state === MatchState.Locked || m.state === MatchState.Resolved
      );
      if (expandable) {
        setExpandedMatchId(expandable.id);
      }
    }
  }, [matches]);

  const handleTabChange = (tab: TabState) => {
    hapticFeedback('selection');
    setActiveTab(tab);
  };

  const isAdmin = profile?.isAdmin ?? false;

  const canExpand = (state: MatchState) =>
    state !== MatchState.Canceled && (state !== MatchState.Upcoming || isAdmin);

  const handleToggleMatch = (match: MatchDto) => {
    if (!canExpand(match.state)) return;
    hapticFeedback('selection');
    setExpandedMatchId(prev => (prev === match.id ? null : match.id));
  };

  const handleBack = () => {
    hapticFeedback('selection');
    onBack();
  };

  const handleMatchRefresh = () => {
    loadData();
  };

  if (isLoading) {
    return (
      <div className="tournament-page">
        <div className="loading-container"><div className="spinner"></div></div>
      </div>
    );
  }

  return (
    <div className="tournament-page">
      <header className="page-header">
        <div className="header-top-row">
          <button className="back-btn" onClick={handleBack}>←</button>
          <h1 className="tournament-title">{tournament.name}</h1>
          {profile && <div className="user-badge">{profile.gameNickname}</div>}
        </div>

        <div className="tabs-bar">
          <button
            className={`tab-btn ${activeTab === 'games' ? 'active' : ''}`}
            onClick={() => handleTabChange('games')}
          >
            Игры
          </button>
          <button
            className={`tab-btn ${activeTab === 'leaders' ? 'active' : ''}`}
            onClick={() => handleTabChange('leaders')}
          >
            Лидеры
          </button>
        </div>
      </header>

      <div className="page-content">
        {activeTab === 'games' && (
          <div className="games-tab">
            {isAdmin && (
              <button
                className="create-match-btn"
                onClick={() => { hapticFeedback('selection'); setShowCreateForm(true); }}
              >
                + Создать игру
              </button>
            )}

            {matches.length === 0 ? (
              <div className="no-matches-card"><p>Игр пока нет</p></div>
            ) : (
              <div className="matches-accordion">
                {matches.map(match => (
                  <MatchCard
                    key={match.id}
                    match={match}
                    isExpanded={expandedMatchId === match.id}
                    canExpand={canExpand(match.state)}
                    onToggle={() => handleToggleMatch(match)}
                    isAdmin={isAdmin}
                    onRefresh={handleMatchRefresh}
                    onResolve={() => setResolvingMatchId(match.id)}
                  />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'leaders' && (
          <LeaderboardTab tournamentId={tournament.id} />
        )}
      </div>

      {showCreateForm && (
        <CreateMatchForm
          tournamentId={tournament.id}
          onSuccess={() => { setShowCreateForm(false); loadData(); }}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      {resolvingMatchId !== null && (
        <ResolveForm
          matchId={resolvingMatchId}
          onSuccess={() => { setResolvingMatchId(null); loadData(); }}
          onCancel={() => setResolvingMatchId(null)}
        />
      )}
    </div>
  );
};
