import React, { useEffect, useState } from 'react';
import { getActiveTournaments, getProfile, getTournamentMatches } from '../lib/api';
import { TournamentDto, UserProfile, MatchDto, MatchState } from '../types';
import { MatchCard } from '../components/MatchCard';
import { LeaderboardPage } from './LeaderboardPage';
import { AdminPage } from './AdminPage';
import { hapticFeedback } from '../lib/telegram';
import './TournamentPage.css';

type PageState = 'tournament' | 'leaderboard' | 'admin';

export const TournamentPage: React.FC = () => {
  const [page, setPage] = useState<PageState>('tournament');
  const [activeTournament, setActiveTournament] = useState<TournamentDto | null>(null);
  const [matches, setMatches] = useState<MatchDto[]>([]);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showHistory, setShowHistory] = useState(false);

  useEffect(() => {
    async function init() {
      try {
        const [tournaments, userProfile] = await Promise.all([
          getActiveTournaments(),
          getProfile()
        ]);
        
        setProfile(userProfile);
        
        if (tournaments.length > 0) {
          const tournament = tournaments[0];
          setActiveTournament(tournament);
          
          const tournamentMatches = await getTournamentMatches(tournament.id);
          // Sort descending
          setMatches(tournamentMatches.sort((a, b) => b.gameNumber - a.gameNumber));
        }
      } catch (err) {
        console.error('Failed to init tournament page:', err);
      } finally {
        setIsLoading(false);
      }
    }
    
    init();
  }, []);

  const handleToggleHistory = () => {
    hapticFeedback('selection');
    setShowHistory(!showHistory);
  };

  const navigateToLeaderboard = () => {
    hapticFeedback('selection');
    setPage('leaderboard');
  };

  // Logic to pick featured match
  let featuredMatch: MatchDto | undefined = undefined;
  
  if (matches.length > 0) {
    if (activeTournament?.currentMatch) {
       featuredMatch = matches.find(m => m.id === activeTournament.currentMatch?.id);
    }
    
    if (!featuredMatch) {
       // Look for first Open or Locked
       featuredMatch = matches.find(m => m.state === MatchState.Open || m.state === MatchState.Locked);
    }
    
    if (!featuredMatch) {
       // Look for upcoming
       featuredMatch = matches.find(m => m.state === MatchState.Upcoming);
    }
    
    if (!featuredMatch) {
       // Just the latest
       featuredMatch = matches[0];
    }
  }

  if (page === 'leaderboard' && activeTournament) {
      return (
          <LeaderboardPage 
            tournamentId={activeTournament.id} 
            onBack={() => setPage('tournament')} 
          />
      );
  }

  if (page === 'admin' && activeTournament) {
    return (
      <AdminPage 
        tournamentId={activeTournament.id} 
        onBack={() => setPage('tournament')} 
      />
    );
  }

  if (isLoading) {
    return (
        <div className="loading-container">
            <div className="spinner"></div>
        </div>
    );
  }

  if (!activeTournament) {
    return (
        <div className="container empty-state">
            <p>Нет активных турниров</p>
        </div>
    );
  }

  return (
    <div className="tournament-page">
      <header className="page-header">
         <div className="tournament-info-container">
            <div>
                 <h1 className="tournament-title">{activeTournament.name}</h1>
                 {activeTournament.description && (
                     <p className="tournament-desc">{activeTournament.description}</p>
                 )}
            </div>
               {profile?.isAdmin && (
                 <button 
                    className="leaderboard-btn" 
                    onClick={() => { hapticFeedback('selection'); setPage('admin'); }}
                    style={{ 
                      marginTop: '8px', 
                      backgroundColor: 'var(--tg-theme-text-color)', 
                      color: 'var(--tg-theme-bg-color)'
                    }}
                 >
                    🛠 Админ панель
                 </button>
               )}
             {profile && <div className="user-badge">{profile.gameNickname}</div>}
         </div>
      </header>

      <div className="page-content">
          <div className="actions-bar">
               <button className="leaderboard-btn" onClick={navigateToLeaderboard}>
                  🏆 Таблица лидеров
               </button>
          </div>

          <div className="featured-section">
              {featuredMatch ? (
                  <MatchCard match={featuredMatch} isCurrent={true} />
              ) : (
                  <div className="no-matches-card">
                      <p>Игр пока нет</p>
                  </div>
              )}
          </div>

          <div className="history-section">
              <button className="history-toggle" onClick={handleToggleHistory}>
                  <span>Все игры ({matches.length})</span>
                  <span className={`arrow ${showHistory ? 'open' : ''}`}>▼</span>
              </button>
              
              {showHistory && (
                  <div className="matches-list">
                      {matches.map(match => {
                          if (match.id === featuredMatch?.id) return null;
                          return (
                              <div key={match.id} className="history-match-item">
                                  <MatchCard 
                                    match={match} 
                                    isCurrent={false} 
                                  />
                              </div>
                          );
                      })}
                  </div>
              )}
          </div>
      </div>
    </div>
  );
};
