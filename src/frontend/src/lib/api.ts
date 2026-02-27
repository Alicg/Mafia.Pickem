import { 
  AuthResponse, 
  UserProfile, 
  TournamentDto, 
  MatchDto, 
  LeaderboardResponse,
  CreateMatchRequest,
  ResolveMatchRequest,
  TournamentStats
} from '../types';
import { isDemoMode } from '../mocks/demo-mode';
import { demoUser, demoTournament, demoTournaments, demoMatches, demoLeaderboard, demoStats } from '../mocks/demo-data';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

let authToken: string | null = null;

export function setAuthToken(token: string): void {
  authToken = token;
}

export function getAuthToken(): string | null {
  return authToken;
}

async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> || {}),
  };

  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`;
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
  if (isDemoMode) return { token: 'demo-token', user: demoUser };
  return apiFetch('/auth/telegram', {
    method: 'POST',
    body: JSON.stringify({ initData }),
  });
}

// Profile
export async function getProfile(): Promise<UserProfile> {
  if (isDemoMode) return demoUser;
  return apiFetch('/me');
}

export async function updateNickname(gameNickname: string): Promise<AuthResponse> {
  if (isDemoMode) return { token: 'demo-token', user: { ...demoUser, gameNickname } };
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

export async function getTournamentMatches(id: number): Promise<MatchDto[]> {
  if (isDemoMode) return [...demoMatches];
  return apiFetch(`/tournaments/${encodeURIComponent(id)}/matches`);
}

// Matches
export async function getMatch(id: number): Promise<MatchDto> {
  if (isDemoMode) return demoMatches.find(m => m.id === id) || demoMatches[0];
  return apiFetch(`/matches/${encodeURIComponent(id)}`);
}

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

export async function getLeaderboard(tournamentId: number): Promise<LeaderboardResponse> {
  if (isDemoMode) return demoLeaderboard;
  const result = await apiFetch<LeaderboardResponse>(`/tournaments/${encodeURIComponent(tournamentId)}/leaderboard`);
  return result;
}

// Admin API
export async function adminCreateMatch(request: CreateMatchRequest): Promise<MatchDto> {
  if (isDemoMode) {
    console.log('[DEMO] adminCreateMatch', request);
    return { id: 99, gameNumber: request.gameNumber, tableNumber: request.tableNumber ?? null, state: 0, myPrediction: null, voteStats: null };
  }
  return apiFetch('/admin/matches', {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export async function adminOpenMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 1 };
  return apiFetch(`/admin/matches/${encodeURIComponent(matchId)}/open`, { method: 'POST' });
}

export async function adminLockMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 2 };
  return apiFetch(`/admin/matches/${encodeURIComponent(matchId)}/lock`, { method: 'POST' });
}

export async function adminResolveMatch(matchId: number, request: ResolveMatchRequest): Promise<MatchDto> {
  if (isDemoMode) {
    console.log('[DEMO] adminResolveMatch', matchId, request);
    return { ...demoMatches[0], id: matchId, state: 3 };
  }
  return apiFetch(`/admin/matches/${encodeURIComponent(matchId)}/resolve`, {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export async function adminCancelMatch(matchId: number): Promise<MatchDto> {
  if (isDemoMode) return { ...demoMatches[0], id: matchId, state: 4 };
  return apiFetch(`/admin/matches/${encodeURIComponent(matchId)}/cancel`, { method: 'POST' });
}

export async function adminPublishState(matchId: number): Promise<void> {
  if (isDemoMode) { console.log('[DEMO] adminPublishState', matchId); return; }
  await apiFetch(`/admin/matches/${encodeURIComponent(matchId)}/publish-state`, { method: 'POST' });
}

export async function adminGetTournamentStats(tournamentId: number): Promise<TournamentStats> {
  if (isDemoMode) return demoStats;
  return apiFetch(`/admin/tournaments/${encodeURIComponent(tournamentId)}/stats`);
}
