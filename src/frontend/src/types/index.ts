export interface AuthResponse {
  user: UserProfile;
}

export interface UserProfile {
  id: number;
  telegramId: number;
  gameNickname: string | null;
  photoUrl: string | null;
  isRegistered: boolean;
  isAdmin: boolean;
}

export interface TournamentDto {
  id: number;
  name: string;
  description: string | null;
  imageUrl: string | null;
  teams: string[];
  operatorUsernames: string[];
  selectedTeamName: string | null;
  canManage: boolean;
  currentMatch: MatchInfo | null;
}

/** Basic match info returned by GET /tournaments/{id}/matches */
export interface MatchInfo {
  id: number;
  gameNumber: number;
  tableNumber: number | null;
  state: MatchState;
}

/** @deprecated Full match DTO used by GET /matches/{id} — prefer MatchInfo + blob + predictions */
export interface MatchDto {
  id: number;
  gameNumber: number;
  tableNumber: number | null;
  state: MatchState;
  myPrediction: PredictionDto | null;
}

/** Map of matchId → PredictionDto returned by GET /tournaments/{id}/my-predictions */
export type PredictionsMap = Record<string, PredictionDto>;

export enum MatchState {
  Upcoming = 0,
  Open = 1,
  Locked = 2,
  Resolved = 3,
  Canceled = 4,
  FirstVoted = 5,
}

export const UNKNOWN_WINNING_SIDE = 255;

/** Last round prediction values */
export enum LastRound {
  None = 0,
  TownClean = 1,   // Победа города — сухая
  TownGuess = 2,   // Победа города — угадайка
  Mafia3v3 = 3,    // Победа мафии — 3в3
  Mafia2v2 = 4,    // Победа мафии — 2в2
  Mafia1v1 = 5,    // Победа мафии — 1в1
}

export const LAST_ROUND_LABELS: Record<number, string> = {
  [LastRound.TownClean]: 'Сухая',
  [LastRound.TownGuess]: 'Другое',
  [LastRound.Mafia3v3]: '3в3',
  [LastRound.Mafia2v2]: '2в2',
  [LastRound.Mafia1v1]: '1в1',
};

export const TOWN_LAST_ROUNDS = [LastRound.TownClean, LastRound.TownGuess];
export const MAFIA_LAST_ROUNDS = [LastRound.Mafia3v3, LastRound.Mafia2v2, LastRound.Mafia1v1];

export interface PredictionDto {
  predictedWinner: number;
  predictedVotedOut: number;
  predictedLastRound: number;
  winnerPoints: number | null;
  votedOutPoints: number | null;
  lastRoundPoints: number | null;
  totalPoints: number | null;
}

export interface LeaderboardResponse {
  entries: LeaderboardEntryDto[];
}

export interface LeaderboardEntryDto {
  userId: number;
  rank: number;
  displayName: string;
  photoUrl: string | null;
  teamName: string | null;
  totalPoints: number;
  correctPredictions: number;
  totalPredictions: number;
}

// Admin Types
export interface CreateMatchRequest {
  tournamentId: number;
  gameNumber: number;
  tableNumber?: number;
  externalMatchRef?: string;
}

export interface CreateTournamentRequest {
  name: string;
  description?: string;
  imageUrl?: string;
  teams: string[];
  operatorUsernames: string[];
}

export interface UpdateTournamentRequest {
  name: string;
  description?: string;
  imageUrl?: string;
  teams: string[];
  operatorUsernames: string[];
}

export interface SelectTournamentTeamRequest {
  teamName: string;
}

export interface TournamentTeamSelectionDto {
  tournamentId: number;
  teamName: string;
}

export interface ResolveMatchRequest {
  winningSide: number;  // 0=Town, 1=Mafia
  votedOutSlots: number[];  // [0]=Nobody, [3,7]=Players
  lastRound: number;  // LastRound enum value
}

export interface SetFirstVotedRequest {
  votedOutSlots: number[];  // [0]=Nobody, [3,7]=Players
}

export interface TournamentStats {
  tournamentId: number;
  totalMatches: number;
  matchesByState: Record<string, number>;
  totalPredictions: number;
}

// Blob polling state
export interface BlobMatchState {
  matchId: number;
  tournamentId: number;
  version: number;
  state: string;
  updatedAt: string;
  tableSize: number;
  totalPredictions: number;
  winnerVotes: {
    town: { count: number; percent: number };
    mafia: { count: number; percent: number };
  } | null;
  votedOutVotes: { slot: number; count: number; percent: number }[] | null;
  lastRoundVotes: { lastRound: number; count: number; percent: number }[] | null;
  matchResult: {
    winningSide: number;   // 0=Town, 1=Mafia, 255=Unknown for FirstVoted
    votedOutSlots: number[];
    lastRound: number;     // LastRound enum value
  } | null;
}
