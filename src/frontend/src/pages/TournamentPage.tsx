import React, { useEffect, useMemo, useRef, useState, useCallback } from 'react';
import { getProfile, getTournamentMatches, getMyPredictions } from '../lib/api';
import { TournamentDto, UserProfile, MatchInfo, MatchState, PredictionsMap, PredictionDto } from '../types';
import { useMatchStates } from '../hooks/useMatchStates';
import { MatchCard } from '../components/MatchCard';
import { LeaderboardTab } from '../components/LeaderboardTab';
import { CreateMatchForm } from '../components/admin/CreateMatchForm';
import { ResolveForm } from '../components/admin/ResolveForm';
import { hapticFeedback } from '../lib/telegram';
import './TournamentPage.css';

const MATCH_LIST_REFRESH_MS = 60_000;

type TabState = 'games' | 'leaders';

interface TournamentPageProps {
  tournament: TournamentDto;
  onBack: () => void;
}

function parseState(s: string): MatchState {
  const lower = s.toLowerCase();
  if (lower === 'upcoming' || lower === '0') return MatchState.Upcoming;
  if (lower === 'open' || lower === '1') return MatchState.Open;
  if (lower === 'locked' || lower === '2') return MatchState.Locked;
  if (lower === 'resolved' || lower === '3') return MatchState.Resolved;
  if (lower === 'canceled' || lower === '4') return MatchState.Canceled;
  return MatchState.Upcoming;
}

export const TournamentPage: React.FC<TournamentPageProps> = ({ tournament, onBack }) => {
  const [activeTab, setActiveTab] = useState<TabState>('games');
  const [matchInfos, setMatchInfos] = useState<MatchInfo[]>([]);
  const [predictions, setPredictions] = useState<PredictionsMap>({});
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [expandedMatchId, setExpandedMatchId] = useState<number | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [resolvingMatchId, setResolvingMatchId] = useState<number | null>(null);

  // Blob polling for all matches
  const matchIds = useMemo(() => matchInfos.map(m => m.id), [matchInfos]);
  const { states: blobStates, refetchMatch } = useMatchStates(matchIds);

  // Initial data load
  const loadInitialData = useCallback(async () => {
    try {
      const [userProfile, matches, preds] = await Promise.all([
        getProfile(),
        getTournamentMatches(tournament.id),
        getMyPredictions(tournament.id),
      ]);
      setProfile(userProfile);
      setMatchInfos(matches.sort((a, b) => a.gameNumber - b.gameNumber));
      setPredictions(preds);
    } catch (err) {
      console.error('Failed to init tournament page:', err);
    } finally {
      setIsLoading(false);
    }
  }, [tournament.id]);

  useEffect(() => {
    loadInitialData();
  }, [loadInitialData]);

  // Refresh match list every 60s to detect new games
  useEffect(() => {
    const interval = setInterval(async () => {
      try {
        const matches = await getTournamentMatches(tournament.id);
        setMatchInfos(matches.sort((a, b) => a.gameNumber - b.gameNumber));
      } catch (err) {
        console.error('Match list refresh failed:', err);
      }
    }, MATCH_LIST_REFRESH_MS);
    return () => clearInterval(interval);
  }, [tournament.id]);

  // Get effective match state (blob overrides API)
  const getEffectiveState = useCallback((matchInfo: MatchInfo): MatchState => {
    const blob = blobStates[matchInfo.id];
    return blob ? parseState(blob.state) : matchInfo.state;
  }, [blobStates]);

  const firstOpenRef = useRef<HTMLDivElement>(null);
  const hasAutoScrolled = useRef(false);

  const firstOpenId = useMemo(() => {
    return matchInfos.find(m => getEffectiveState(m) === MatchState.Open)?.id ?? null;
  }, [matchInfos, getEffectiveState]);

  // Auto-expand the first Open match
  useEffect(() => {
    if (matchInfos.length > 0 && expandedMatchId === null && firstOpenId !== null) {
      setExpandedMatchId(firstOpenId);
    }
  }, [matchInfos, blobStates, firstOpenId]);

  // Scroll to the first Open match after initial expand
  useEffect(() => {
    if (!hasAutoScrolled.current && expandedMatchId !== null && firstOpenRef.current) {
      hasAutoScrolled.current = true;
      setTimeout(() => {
        firstOpenRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }, 150);
    }
  }, [expandedMatchId]);

  const handleTabChange = (tab: TabState) => {
    hapticFeedback('selection');
    setActiveTab(tab);
  };

  const isAdmin = profile?.isAdmin ?? false;

  const canExpand = (state: MatchState) =>
    state !== MatchState.Canceled && (state !== MatchState.Upcoming || isAdmin);

  const handleToggleMatch = (matchInfo: MatchInfo) => {
    const state = getEffectiveState(matchInfo);
    if (!canExpand(state)) return;
    hapticFeedback('selection');
    setExpandedMatchId(prev => (prev === matchInfo.id ? null : matchInfo.id));
  };

  const handleBack = () => {
    hapticFeedback('selection');
    onBack();
  };

  // Update prediction locally (no API refetch needed)
  const handlePredictionChange = useCallback((matchId: number, prediction: PredictionDto | null) => {
    setPredictions(prev => {
      const next = { ...prev };
      if (prediction) {
        next[String(matchId)] = prediction;
      } else {
        delete next[String(matchId)];
      }
      return next;
    });
  }, []);

  // Re-fetch predictions when any match transitions to Resolved
  // so that calculated scores (points) are loaded from the API.
  const prevBlobStatesRef = React.useRef<Record<number, string>>({});
  useEffect(() => {
    const prev = prevBlobStatesRef.current;
    let needRefetch = false;
    for (const [idStr, blob] of Object.entries(blobStates)) {
      if (!blob) continue;
      const st = blob.state.toLowerCase();
      const prevSt = prev[Number(idStr)];
      if (st === 'resolved' && prevSt !== 'resolved') {
        needRefetch = true;
      }
      prev[Number(idStr)] = st;
    }
    if (needRefetch) {
      getMyPredictions(tournament.id)
        .then(preds => setPredictions(preds))
        .catch(err => console.error('Failed to refresh predictions:', err));
    }
  }, [blobStates, tournament.id]);

  // Refresh match list (used by admin actions)
  const refreshMatchList = useCallback(async () => {
    try {
      const matches = await getTournamentMatches(tournament.id);
      setMatchInfos(matches.sort((a, b) => a.gameNumber - b.gameNumber));
    } catch (err) {
      console.error('Match list refresh failed:', err);
    }
  }, [tournament.id]);

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
          <button className="back-btn" onClick={handleBack}>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="15 18 9 12 15 6" />
            </svg>
          </button>
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

            {matchInfos.length === 0 ? (
              <div className="no-matches-card"><p>Игр пока нет</p></div>
            ) : (
              <div className="matches-accordion">
                {matchInfos.map(mi => (
                  <div key={mi.id} ref={mi.id === firstOpenId ? firstOpenRef : undefined}>
                    <MatchCard
                      matchInfo={mi}
                      blobState={blobStates[mi.id] ?? null}
                      prediction={predictions[String(mi.id)] ?? null}
                      isExpanded={expandedMatchId === mi.id}
                      canExpand={canExpand(getEffectiveState(mi))}
                      onToggle={() => handleToggleMatch(mi)}
                      isAdmin={isAdmin}
                      onPredictionChange={(p) => handlePredictionChange(mi.id, p)}
                      onRefresh={refreshMatchList}
                      onResolve={() => setResolvingMatchId(mi.id)}
                      onRefetchState={() => refetchMatch(mi.id)}
                    />
                  </div>
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
          onSuccess={() => { setShowCreateForm(false); refreshMatchList(); }}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      {resolvingMatchId !== null && (
        <ResolveForm
          matchId={resolvingMatchId}
          onSuccess={() => { setResolvingMatchId(null); refreshMatchList(); }}
          onCancel={() => setResolvingMatchId(null)}
        />
      )}
    </div>
  );
};
