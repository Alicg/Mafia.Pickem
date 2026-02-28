export interface AuthResponse {
  token: string;
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
}

export interface PredictionDto {
  predictedWinner: number;
  predictedVotedOut: number;
  winnerPoints: number | null;
  votedOutPoints: number | null;
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
}

export interface ResolveMatchRequest {
  winningSide: number;  // 0=Town, 1=Mafia
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
  matchResult: {
    winningSide: number;   // 0=Town, 1=Mafia
    votedOutSlots: number[];
  } | null;
}
