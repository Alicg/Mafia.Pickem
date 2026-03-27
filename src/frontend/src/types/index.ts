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

export type OverlayPanelSide = 'left' | 'right';

export interface OverlayStackPanelLayout {
  edgeOffset: number;
  topOffset: number;
}

export interface OverlayBlockPlacement {
  panel: OverlayPanelSide;
  isVisible: boolean;
  dynamicDisplay: OverlayDynamicDisplaySettings;
}

export interface OverlayDynamicDisplaySettings {
  enabled: boolean;
  intervalSeconds: number;
  visibleDurationSeconds: number;
  animationDurationMs: number;
}

export interface OverlayThemeSettings {
  fillColorStart: string;
  fillColorEnd: string;
  fillOpacity: number;
  useGradient: boolean;
}

export type TournamentOverlayType = 'classic' | 'viewer-sympathy';

export interface ViewerSympathyOverlayBlockSettings {
  horizontalOffset: number;
  verticalOffset: number;
  scale: number;
  dynamicDisplay: OverlayDynamicDisplaySettings;
}

export interface TournamentOverlaySettings {
  overlayType: TournamentOverlayType;
  hideBlocksByPhase: boolean;
  theme: OverlayThemeSettings;
  leftPanel: OverlayStackPanelLayout;
  rightPanel: OverlayStackPanelLayout;
  summaryBlock: OverlayBlockPlacement;
  firstVoteBlock: OverlayBlockPlacement;
  lastRoundBlock: OverlayBlockPlacement;
  footerBlock: OverlayBlockPlacement;
  viewerSympathyBlock: ViewerSympathyOverlayBlockSettings;
}

export const DEFAULT_TOURNAMENT_OVERLAY_SETTINGS: TournamentOverlaySettings = {
  overlayType: 'classic',
  hideBlocksByPhase: true,
  theme: {
    fillColorStart: '#163A61',
    fillColorEnd: '#0B1F3A',
    fillOpacity: 92,
    useGradient: true,
  },
  leftPanel: {
    edgeOffset: 15,
    topOffset: 138,
  },
  rightPanel: {
    edgeOffset: 15,
    topOffset: 394,
  },
  summaryBlock: {
    panel: 'left',
    isVisible: true,
    dynamicDisplay: {
      enabled: false,
      intervalSeconds: 30,
      visibleDurationSeconds: 8,
      animationDurationMs: 420,
    },
  },
  firstVoteBlock: {
    panel: 'right',
    isVisible: true,
    dynamicDisplay: {
      enabled: false,
      intervalSeconds: 30,
      visibleDurationSeconds: 8,
      animationDurationMs: 420,
    },
  },
  lastRoundBlock: {
    panel: 'left',
    isVisible: true,
    dynamicDisplay: {
      enabled: false,
      intervalSeconds: 30,
      visibleDurationSeconds: 8,
      animationDurationMs: 420,
    },
  },
  footerBlock: {
    panel: 'left',
    isVisible: true,
    dynamicDisplay: {
      enabled: false,
      intervalSeconds: 30,
      visibleDurationSeconds: 8,
      animationDurationMs: 420,
    },
  },
  viewerSympathyBlock: {
    horizontalOffset: 0,
    verticalOffset: 24,
    scale: 10,
    dynamicDisplay: {
      enabled: false,
      intervalSeconds: 30,
      visibleDurationSeconds: 8,
      animationDurationMs: 420,
    },
  },
};

export function cloneTournamentOverlaySettings(settings?: TournamentOverlaySettings | null): TournamentOverlaySettings {
  const source = settings ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS;

  return {
    overlayType: source.overlayType ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.overlayType,
    hideBlocksByPhase: source.hideBlocksByPhase,
    theme: {
      fillColorStart: source.theme.fillColorStart,
      fillColorEnd: source.theme.fillColorEnd,
      fillOpacity: source.theme.fillOpacity,
      useGradient: source.theme.useGradient,
    },
    leftPanel: { ...source.leftPanel },
    rightPanel: { ...source.rightPanel },
    summaryBlock: {
      panel: source.summaryBlock?.panel ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.summaryBlock.panel,
      isVisible: source.summaryBlock?.isVisible ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.summaryBlock.isVisible,
      dynamicDisplay: {
        enabled: source.summaryBlock?.dynamicDisplay?.enabled ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.summaryBlock.dynamicDisplay.enabled,
        intervalSeconds: source.summaryBlock?.dynamicDisplay?.intervalSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.summaryBlock.dynamicDisplay.intervalSeconds,
        visibleDurationSeconds: source.summaryBlock?.dynamicDisplay?.visibleDurationSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.summaryBlock.dynamicDisplay.visibleDurationSeconds,
        animationDurationMs: source.summaryBlock?.dynamicDisplay?.animationDurationMs ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.summaryBlock.dynamicDisplay.animationDurationMs,
      },
    },
    firstVoteBlock: {
      panel: source.firstVoteBlock?.panel ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.firstVoteBlock.panel,
      isVisible: source.firstVoteBlock?.isVisible ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.firstVoteBlock.isVisible,
      dynamicDisplay: {
        enabled: source.firstVoteBlock?.dynamicDisplay?.enabled ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.firstVoteBlock.dynamicDisplay.enabled,
        intervalSeconds: source.firstVoteBlock?.dynamicDisplay?.intervalSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.firstVoteBlock.dynamicDisplay.intervalSeconds,
        visibleDurationSeconds: source.firstVoteBlock?.dynamicDisplay?.visibleDurationSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.firstVoteBlock.dynamicDisplay.visibleDurationSeconds,
        animationDurationMs: source.firstVoteBlock?.dynamicDisplay?.animationDurationMs ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.firstVoteBlock.dynamicDisplay.animationDurationMs,
      },
    },
    lastRoundBlock: {
      panel: source.lastRoundBlock?.panel ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.lastRoundBlock.panel,
      isVisible: source.lastRoundBlock?.isVisible ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.lastRoundBlock.isVisible,
      dynamicDisplay: {
        enabled: source.lastRoundBlock?.dynamicDisplay?.enabled ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.lastRoundBlock.dynamicDisplay.enabled,
        intervalSeconds: source.lastRoundBlock?.dynamicDisplay?.intervalSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.lastRoundBlock.dynamicDisplay.intervalSeconds,
        visibleDurationSeconds: source.lastRoundBlock?.dynamicDisplay?.visibleDurationSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.lastRoundBlock.dynamicDisplay.visibleDurationSeconds,
        animationDurationMs: source.lastRoundBlock?.dynamicDisplay?.animationDurationMs ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.lastRoundBlock.dynamicDisplay.animationDurationMs,
      },
    },
    footerBlock: {
      panel: source.footerBlock?.panel ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.footerBlock.panel,
      isVisible: source.footerBlock?.isVisible ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.footerBlock.isVisible,
      dynamicDisplay: {
        enabled: source.footerBlock?.dynamicDisplay?.enabled ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.footerBlock.dynamicDisplay.enabled,
        intervalSeconds: source.footerBlock?.dynamicDisplay?.intervalSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.footerBlock.dynamicDisplay.intervalSeconds,
        visibleDurationSeconds: source.footerBlock?.dynamicDisplay?.visibleDurationSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.footerBlock.dynamicDisplay.visibleDurationSeconds,
        animationDurationMs: source.footerBlock?.dynamicDisplay?.animationDurationMs ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.footerBlock.dynamicDisplay.animationDurationMs,
      },
    },
    viewerSympathyBlock: {
      horizontalOffset: source.viewerSympathyBlock?.horizontalOffset ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.horizontalOffset,
      verticalOffset: source.viewerSympathyBlock?.verticalOffset ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.verticalOffset,
      scale: source.viewerSympathyBlock?.scale ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.scale,
      dynamicDisplay: {
        enabled: source.viewerSympathyBlock?.dynamicDisplay?.enabled ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.dynamicDisplay.enabled,
        intervalSeconds: source.viewerSympathyBlock?.dynamicDisplay?.intervalSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.dynamicDisplay.intervalSeconds,
        visibleDurationSeconds: source.viewerSympathyBlock?.dynamicDisplay?.visibleDurationSeconds ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.dynamicDisplay.visibleDurationSeconds,
        animationDurationMs: source.viewerSympathyBlock?.dynamicDisplay?.animationDurationMs ?? DEFAULT_TOURNAMENT_OVERLAY_SETTINGS.viewerSympathyBlock.dynamicDisplay.animationDurationMs,
      },
    },
  };
}

export interface TournamentDto {
  id: number;
  name: string;
  description: string | null;
  imageUrl: string | null;
  teams: string[];
  operatorUsernames: string[];
  selectedTeamName: string | null;
  visibleOnHomePage: boolean;
  showTeamSelection: boolean;
  overlaySettings: TournamentOverlaySettings;
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
  visibleOnHomePage: boolean;
  showTeamSelection: boolean;
  overlaySettings: TournamentOverlaySettings;
}

export interface UpdateTournamentRequest {
  name: string;
  description?: string;
  imageUrl?: string;
  teams: string[];
  operatorUsernames: string[];
  visibleOnHomePage: boolean;
  showTeamSelection: boolean;
  overlaySettings: TournamentOverlaySettings;
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
