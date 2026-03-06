import { 
  AuthResponse, 
  UserProfile, 
  TournamentDto, 
  MatchDto,
  MatchInfo,
  PredictionsMap,
  LeaderboardResponse,
  CreateMatchRequest,
  CreateTournamentRequest,
  ResolveMatchRequest,
  TournamentStats
} from '../types';
import { isDemoMode } from '../mocks/demo-mode';
import { demoUser, demoTournament, demoTournaments, demoMatchInfos, demoMatches, demoPredictionsMap, demoLeaderboard, demoStats } from '../mocks/demo-data';
import { getInitData } from './telegram';

// Browser runtime base: where the browser should send API requests.
const API_BASE_URL = import.meta.env.VITE_BROWSER_API_BASE_URL || '/api';
// Browser runtime base: where the browser should send blob requests.
const BLOB_BASE_URL = import.meta.env.VITE_BROWSER_BLOB_BASE_URL || '/blob';
const isDevAuth = import.meta.env.VITE_DEV_AUTH === 'true';

async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> || {}),
  };

  if (isDevAuth) {
    headers['X-Dev-Auth'] = 'true';
  } else {
    const initData = getInitData();
    if (initData) {
      headers['X-Telegram-Init-Data'] = initData;
    }
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `HTTP ${response.status}`);
  }

  // Handle empty response (e.g. 204 No Content, check if body is expected)
  // For now assume JSON if content-length > 0 or not 204
  const text = await response.text();
  return text ? JSON.parse(text) : ({} as T);
}

// Auth
export async function authenticateTelegram(initData: string): Promise<AuthResponse> {
  if (isDemoMode) return { user: demoUser };
  return apiFetch('/auth/telegram', {
    method: 'POST',
    body: JSON.stringify({ initData }),
  });
}

export async function devAuthenticate(): Promise<AuthResponse> {
  return apiFetch('/auth/dev', { method: 'POST' });
}

// Profile
export async function getProfile(): Promise<UserProfile> {
  if (isDemoMode) return demoUser;
  return apiFetch('/me');
}

export async function updateNickname(gameNickname: string): Promise<AuthResponse> {
  if (isDemoMode) return { user: { ...demoUser, gameNickname } };
  return apiFetch('/me/nickname', {
    method: 'POST',
    body: JSON.stringify({ gameNickname }),
  });
}

// Tournaments
export async function getActiveTournaments(): Promise<TournamentDto[]> {
  if (isDemoMode) return [...demoTournaments];
  return apiFetch('/tournaments/active');
}

export async function getTournament(id: number): Promise<TournamentDto> {
  if (isDemoMode) return demoTournament;
  return apiFetch(`/tournaments/${encodeURIComponent(id)}`);
}

export async function getTournamentMatches(id: number): Promise<MatchInfo[]> {
  if (isDemoMode) return [...demoMatchInfos];
  return apiFetch(`/tournaments/${encodeURIComponent(id)}/matches`);
}

export async function getMyPredictions(tournamentId: number): Promise<PredictionsMap> {
  if (isDemoMode) return { ...demoPredictionsMap };
  return apiFetch(`/tournaments/${encodeURIComponent(tournamentId)}/my-predictions`);
}

// Matches
export async function submitPrediction(matchId: number, predictedWinner: number, predictedVotedOut: number): Promise<void> {
  if (isDemoMode) {
    console.log('[DEMO] submitPrediction', { matchId, predictedWinner, predictedVotedOut });
    return;
  }
  await apiFetch(`/matches/${encodeURIComponent(matchId)}/predict`, {
    method: 'POST',
    body: JSON.stringify({ predictedWinner, predictedVotedOut }),
  });
}

export async function deletePrediction(matchId: number): Promise<void> {
  if (isDemoMode) {
    console.log('[DEMO] deletePrediction', { matchId });
    return;
  }
  await apiFetch(`/matches/${encodeURIComponent(matchId)}/predict`, {
    method: 'DELETE',
  });
}

export async function getLeaderboard(tournamentId: number): Promise<LeaderboardResponse> {
  if (isDemoMode) return demoLeaderboard;
  const res = await fetch(`${BLOB_BASE_URL}/leaderboard-${tournamentId}.json?t=${Date.now()}`);
  if (!res.ok) {
    if (res.status === 404) return { entries: [] };
    throw new Error(`Leaderboard fetch failed: ${res.status}`);
  }
  return res.json();
}

// Admin API
export async function adminCreateTournament(request: CreateTournamentRequest): Promise<TournamentDto> {
  if (isDemoMode) {
    console.log('[DEMO] adminCreateTournament', request);
    return { id: 99, name: request.name, description: request.description ?? null, imageUrl: request.imageUrl ?? null, currentMatch: null };
  }
  return apiFetch('/manage/tournaments', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export async function adminCreateMatch(request: CreateMatchRequest): Promise<MatchDto> {
  if (isDemoMode) {
    console.log('[DEMO] adminCreateMatch', request);
    return { id: 99, gameNumber: request.gameNumber, tableNumber: request.tableNumber ?? null, state: 0, myPrediction: null };
  }
  return apiFetch('/manage/matches', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export async function adminOpenMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 1 };
  return apiFetch(`/manage/open-match/${encodeURIComponent(matchId)}`, { method: 'POST' });
}

export async function adminRevertToUpcoming(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 0 };
  return apiFetch(`/manage/revert-to-upcoming/${encodeURIComponent(matchId)}`, { method: 'POST' });
}

export async function adminLockMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 2 };
  return apiFetch(`/manage/lock-match/${encodeURIComponent(matchId)}`, { method: 'POST' });
}

export async function adminReopenMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 1 };
  return apiFetch(`/manage/reopen-match/${encodeURIComponent(matchId)}`, { method: 'POST' });
}

export async function adminResolveMatch(matchId: number, request: ResolveMatchRequest): Promise<MatchDto> {
  if (isDemoMode) {
    console.log('[DEMO] adminResolveMatch', matchId, request);
    return { ...demoMatches[0], id: matchId, state: 3 };
  }
  return apiFetch(`/manage/resolve-match/${encodeURIComponent(matchId)}`, {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export async function adminUnresolveMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) {
    console.log('[DEMO] adminUnresolveMatch', matchId);
    return { ...demoMatches[0], id: matchId, state: 2 };
  }
  return apiFetch(`/manage/unresolve-match/${encodeURIComponent(matchId)}`, { method: 'POST' });
}

export async function adminDeleteMatch(matchId: number): Promise<void> {
  if (isDemoMode) { console.log('[DEMO] adminDeleteMatch', matchId); return; }
  await apiFetch(`/manage/matches/${encodeURIComponent(matchId)}`, { method: 'DELETE' });
}

export async function adminPublishState(matchId: number): Promise<void> {
  if (isDemoMode) { console.log('[DEMO] adminPublishState', matchId); return; }
  await apiFetch(`/manage/publish-match-state/${encodeURIComponent(matchId)}`, { method: 'POST' });
}

export async function adminGetTournamentStats(tournamentId: number): Promise<TournamentStats> {
  if (isDemoMode) return demoStats;
  return apiFetch(`/manage/tournament-stats/${encodeURIComponent(tournamentId)}`);
}
