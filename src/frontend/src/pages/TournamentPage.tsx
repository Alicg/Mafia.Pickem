import React, { useEffect, useMemo, useRef, useState, useCallback } from 'react';
import { adminGetTournamentStats, adminUpdateTournamentOverlayClientState, getProfile, getTournamentMatches, getMyPredictions, selectTournamentTeam } from '../lib/api';
import { TournamentDto, UserProfile, MatchInfo, MatchState, PredictionsMap, PredictionDto, TournamentStats } from '../types';
import { useMatchStates } from '../hooks/useMatchStates';
import { MatchCard } from '../components/MatchCard';
import { LeaderboardTab } from '../components/LeaderboardTab';
import { CreateTournamentForm } from '../components/admin/CreateTournamentForm';
import { CreateMatchForm } from '../components/admin/CreateMatchForm';
import { ResolveForm } from '../components/admin/ResolveForm';
import { SetFirstVotedForm } from '../components/admin/SetFirstVotedForm';
import { hapticFeedback } from '../lib/telegram';
import './TournamentPage.css';

const MATCH_LIST_REFRESH_MS = 60_000;
const OVERLAY_STATE_HEARTBEAT_MS = 10_000;

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
  if (lower === 'firstvoted' || lower === '5') return MatchState.FirstVoted;
  return MatchState.Upcoming;
}

function getOverlayStatePriority(state: MatchState): number {
  if (state === MatchState.Open) return 0;
  if (state === MatchState.Locked) return 1;
  if (state === MatchState.FirstVoted) return 2;
  return Number.MAX_SAFE_INTEGER;
}

export const TournamentPage: React.FC<TournamentPageProps> = ({ tournament, onBack }) => {
  const [activeTab, setActiveTab] = useState<TabState>('games');
  const [editableTournament, setEditableTournament] = useState<TournamentDto>(tournament);
  const [matchInfos, setMatchInfos] = useState<MatchInfo[]>([]);
  const [predictions, setPredictions] = useState<PredictionsMap>({});
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [selectedTeamName, setSelectedTeamName] = useState<string | null>(tournament.selectedTeamName);
  const [pendingTeamName, setPendingTeamName] = useState<string>('');
  const [teamSelectionError, setTeamSelectionError] = useState<string | null>(null);
  const [isSavingTeamSelection, setIsSavingTeamSelection] = useState(false);
  const [tournamentStats, setTournamentStats] = useState<TournamentStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [expandedMatchId, setExpandedMatchId] = useState<number | null>(null);
  const [showTournamentSettings, setShowTournamentSettings] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [resolvingMatch, setResolvingMatch] = useState<{ matchId: number; currentState: MatchState } | null>(null);
  const [firstVotedMatchId, setFirstVotedMatchId] = useState<number | null>(null);
  const canManage = tournament.canManage;
  const requiresTeamSelection = tournament.showTeamSelection && tournament.teams.length > 0 && !selectedTeamName;

  useEffect(() => {
    setEditableTournament(tournament);
  }, [tournament]);

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

      // Admin-only tournament stats shown inline on the main tournament page.
      if (canManage) {
        try {
          const stats = await adminGetTournamentStats(tournament.id);
          setTournamentStats(stats);
        } catch (err) {
          console.error('Failed to load tournament stats:', err);
          setTournamentStats(null);
        }
      } else {
        setTournamentStats(null);
      }
    } catch (err) {
      console.error('Failed to init tournament page:', err);
    } finally {
      setIsLoading(false);
    }
  }, [canManage, tournament.id]);

  useEffect(() => {
    loadInitialData();
  }, [loadInitialData]);

  // Refresh match list every 60s to detect new games
  useEffect(() => {
    const interval = setInterval(async () => {
      try {
        const matches = await getTournamentMatches(tournament.id);
        setMatchInfos(matches.sort((a, b) => a.gameNumber - b.gameNumber));

        if (canManage) {
          try {
            const stats = await adminGetTournamentStats(tournament.id);
            setTournamentStats(stats);
          } catch {
            // Keep existing UI state when stats endpoint is temporarily unavailable.
          }
        }
      } catch (err) {
        console.error('Match list refresh failed:', err);
      }
    }, MATCH_LIST_REFRESH_MS);
    return () => clearInterval(interval);
  }, [canManage, tournament.id]);

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

  const activeOverlayMatchId = useMemo(() => {
    let selected: MatchInfo | null = null;
    let selectedPriority = Number.MAX_SAFE_INTEGER;

    for (const matchInfo of matchInfos) {
      const state = getEffectiveState(matchInfo);
      const priority = getOverlayStatePriority(state);
      if (priority === Number.MAX_SAFE_INTEGER) {
        continue;
      }

      if (
        selected == null ||
        priority < selectedPriority ||
        (priority === selectedPriority && matchInfo.id > selected.id)
      ) {
        selected = matchInfo;
        selectedPriority = priority;
      }
    }

    return selected?.id ?? null;
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

  const getStateCount = useCallback((stateName: string): number => {
    if (!tournamentStats?.matchesByState) return 0;

    const entries = Object.entries(tournamentStats.matchesByState);
    const found = entries.find(([key]) => key.toLowerCase() === stateName.toLowerCase());
    return found ? found[1] : 0;
  }, [tournamentStats]);

  const canExpand = (state: MatchState) =>
    state !== MatchState.Canceled && (state !== MatchState.Upcoming || canManage);

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

  const handleTeamSelectionSubmit = useCallback(async () => {
    if (!pendingTeamName) {
      setTeamSelectionError('Выберите команду из списка.');
      return;
    }

    try {
      setIsSavingTeamSelection(true);
      setTeamSelectionError(null);
      const result = await selectTournamentTeam(tournament.id, { teamName: pendingTeamName });
      setSelectedTeamName(result.teamName);
      hapticFeedback('success');
    } catch (err) {
      console.error('Failed to select tournament team:', err);
      setTeamSelectionError(err instanceof Error ? err.message : 'Не удалось сохранить выбор команды');
      hapticFeedback('error');
    } finally {
      setIsSavingTeamSelection(false);
    }
  }, [pendingTeamName, tournament.id]);

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

  useEffect(() => {
    if (!canManage) {
      return;
    }

    let isDisposed = false;

    const publishOverlayState = async () => {
      try {
        await adminUpdateTournamentOverlayClientState(tournament.id, activeOverlayMatchId);
      } catch (err) {
        if (!isDisposed) {
          console.error('Failed to update overlay client state:', err);
        }
      }
    };

    void publishOverlayState();
    const intervalId = window.setInterval(() => {
      void publishOverlayState();
    }, OVERLAY_STATE_HEARTBEAT_MS);

    return () => {
      isDisposed = true;
      window.clearInterval(intervalId);
    };
  }, [activeOverlayMatchId, canManage, tournament.id]);

  // Refresh match list (used by admin actions)
  const refreshMatchList = useCallback(async () => {
    try {
      const matches = await getTournamentMatches(tournament.id);
      setMatchInfos(matches.sort((a, b) => a.gameNumber - b.gameNumber));

      if (canManage) {
        try {
          const stats = await adminGetTournamentStats(tournament.id);
          setTournamentStats(stats);
        } catch {
          // Keep existing UI state when stats endpoint is temporarily unavailable.
        }
      }
    } catch (err) {
      console.error('Match list refresh failed:', err);
    }
  }, [canManage, tournament.id]);

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
          {profile && <div className="user-badge">{selectedTeamName ? `${profile.gameNickname} · ${selectedTeamName}` : profile.gameNickname}</div>}
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
            {canManage && tournamentStats && (
              <div className="admin-stats-card">
                <div className="admin-stats-row">
                  <div className="admin-stat-item">
                    <div className="admin-stat-value">{tournamentStats.totalPredictions}</div>
                    <div className="admin-stat-label">Прогнозов</div>
                  </div>
                  <div className="admin-stat-item">
                    <div className="admin-stat-value">{tournamentStats.totalMatches}</div>
                    <div className="admin-stat-label">Игр</div>
                  </div>
                </div>
                <div className="admin-state-chips">
                  <span className="admin-state-chip">Upcoming: {getStateCount('Upcoming')}</span>
                  <span className="admin-state-chip">Open: {getStateCount('Open')}</span>
                  <span className="admin-state-chip">Locked: {getStateCount('Locked')}</span>
                  <span className="admin-state-chip">Resolved: {getStateCount('Resolved')}</span>
                </div>
              </div>
            )}

            {canManage && (
              <div className="manage-actions">
                <button
                  className="create-match-btn create-match-btn--secondary"
                  onClick={() => { hapticFeedback('selection'); setShowTournamentSettings(true); }}
                >
                  Настройки турнира
                </button>
                <button
                  className="create-match-btn"
                  onClick={() => { hapticFeedback('selection'); setShowCreateForm(true); }}
                >
                  + Создать игру
                </button>
              </div>
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
                      canManage={canManage}
                      onPredictionChange={(p) => handlePredictionChange(mi.id, p)}
                      onRefresh={refreshMatchList}
                      onResolve={(currentState) => setResolvingMatch({ matchId: mi.id, currentState })}
                      onSetFirstVoted={() => setFirstVotedMatchId(mi.id)}
                      onRefetchState={() => refetchMatch(mi.id)}
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'leaders' && (
          <LeaderboardTab tournamentId={tournament.id} currentUserId={profile?.id ?? 0} />
        )}
      </div>

      {showCreateForm && (
        <CreateMatchForm
          tournamentId={tournament.id}
          onSuccess={() => { setShowCreateForm(false); refreshMatchList(); }}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      {showTournamentSettings && (
        <CreateTournamentForm
          tournament={editableTournament}
          overlaySettingsOnly={true}
          onSuccess={(updatedTournament) => {
            if (updatedTournament) {
              setEditableTournament(updatedTournament);
            }
            setShowTournamentSettings(false);
          }}
          onCancel={() => setShowTournamentSettings(false)}
        />
      )}

      {resolvingMatch !== null && (
        <ResolveForm
          matchId={resolvingMatch.matchId}
          currentState={resolvingMatch.currentState}
          onSuccess={() => { setResolvingMatch(null); refreshMatchList(); }}
          onCancel={() => setResolvingMatch(null)}
        />
      )}

      {firstVotedMatchId !== null && (
        <SetFirstVotedForm
          matchId={firstVotedMatchId}
          onSuccess={() => { setFirstVotedMatchId(null); refreshMatchList(); }}
          onCancel={() => setFirstVotedMatchId(null)}
        />
      )}

      {requiresTeamSelection && (
        <div className="team-selection-overlay">
          <div className="team-selection-modal">
            <div className="team-selection-kicker">Выбор команды обязателен</div>
            <h2>За кого вы болеете в этом турнире?</h2>
            <p className="team-selection-copy">
              Выберите команду один раз перед участием в турнире. Часть заработанных баллов в угадайке будут зачислены выбранному участнику дюжины в фан-баттле. После подтверждения изменить выбор будет нельзя, поэтому выбирайте внимательно.
            </p>

            <label className="team-selection-label" htmlFor="team-select">
              Команда
            </label>
            <select
              id="team-select"
              className="team-selection-select"
              value={pendingTeamName}
              onChange={(event) => setPendingTeamName(event.target.value)}
              disabled={isSavingTeamSelection}
            >
              <option value="">Выберите команду</option>
              {tournament.teams.map((team) => (
                <option key={team} value={team}>{team}</option>
              ))}
            </select>

            <div className="team-selection-warning">
              Подтвердите только тот вариант, за который действительно хотите выступать весь турнир.
            </div>

            {teamSelectionError && <div className="team-selection-error">{teamSelectionError}</div>}

            <div className="team-selection-actions">
              <button type="button" className="team-selection-back" onClick={handleBack} disabled={isSavingTeamSelection}>
                Вернуться
              </button>
              <button type="button" className="team-selection-submit" onClick={handleTeamSelectionSubmit} disabled={isSavingTeamSelection || !pendingTeamName}>
                {isSavingTeamSelection ? 'Сохраняем...' : 'Подтвердить команду'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
