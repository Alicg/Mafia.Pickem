import {
  UserProfile,
  TournamentDto,
  MatchDto,
  MatchInfo,
  MatchState,
  PredictionsMap,
  LeaderboardResponse,
  TournamentStats,
  BlobMatchState,
} from '../types';

export const demoUser: UserProfile = {
  id: 1,
  telegramId: 123456789,
  gameNickname: 'МафиозоДжо',
  photoUrl: null,
  isRegistered: true,
  isAdmin: true,
};

export const demoMatches: MatchDto[] = [
  {
    id: 1,
    gameNumber: 1,
    tableNumber: 1,
    state: MatchState.Resolved,
    myPrediction: {
      predictedWinner: 0,
      predictedVotedOut: 3,
      winnerPoints: 10,
      votedOutPoints: 5,
      totalPoints: 15,
    },
  },
  {
    id: 2,
    gameNumber: 2,
    tableNumber: 1,
    state: MatchState.Resolved,
    myPrediction: {
      predictedWinner: 1,
      predictedVotedOut: 7,
      winnerPoints: 0,
      votedOutPoints: 0,
      totalPoints: 0,
    },
  },
  {
    id: 3,
    gameNumber: 3,
    tableNumber: 2,
    state: MatchState.Open,
    myPrediction: null,
  },
  {
    id: 4,
    gameNumber: 4,
    tableNumber: 2,
    state: MatchState.Upcoming,
    myPrediction: null,
  },
  {
    id: 5,
    gameNumber: 5,
    tableNumber: 1,
    state: MatchState.Locked,
    myPrediction: {
      predictedWinner: 0,
      predictedVotedOut: 5,
      winnerPoints: null,
      votedOutPoints: null,
      totalPoints: null,
    },
  },
];

export const demoTournament: TournamentDto = {
  id: 1,
  name: 'Мафия Кубок Зимы 2026',
  description: 'Зимний турнир по мафии — сделай свой прогноз!',
  imageUrl: null,
  currentMatch: { id: 3, gameNumber: 3, tableNumber: 2, state: MatchState.Open }, // Game #3
};

export const demoTournaments: TournamentDto[] = [
  demoTournament,
  {
    id: 2,
    name: 'Весенний Чемпионат 2026',
    description: 'Открытый чемпионат города — регистрация открыта',
    imageUrl: null,
    currentMatch: null,
  },
];

export const demoLeaderboard: LeaderboardResponse = {
  entries: [
    { rank: 1, displayName: 'ШерлокХолмс', photoUrl: null, totalPoints: 85, correctPredictions: 12, totalPredictions: 15, isCurrentUser: false },
    { rank: 2, displayName: 'Детектив007', photoUrl: null, totalPoints: 72, correctPredictions: 10, totalPredictions: 14, isCurrentUser: false },
    { rank: 3, displayName: 'МафиозоДжо', photoUrl: null, totalPoints: 65, correctPredictions: 9, totalPredictions: 15, isCurrentUser: true },
    { rank: 4, displayName: 'НочнойДозор', photoUrl: null, totalPoints: 58, correctPredictions: 8, totalPredictions: 13, isCurrentUser: false },
    { rank: 5, displayName: 'КомиссарРекс', photoUrl: null, totalPoints: 51, correctPredictions: 7, totalPredictions: 12, isCurrentUser: false },
    { rank: 6, displayName: 'ДонКорлеоне', photoUrl: null, totalPoints: 44, correctPredictions: 6, totalPredictions: 14, isCurrentUser: false },
    { rank: 7, displayName: 'МирныйЖитель', photoUrl: null, totalPoints: 38, correctPredictions: 5, totalPredictions: 11, isCurrentUser: false },
    { rank: 8, displayName: 'Провидец', photoUrl: null, totalPoints: 30, correctPredictions: 4, totalPredictions: 10, isCurrentUser: false },
    { rank: 9, displayName: 'ТихийОмут', photoUrl: null, totalPoints: 22, correctPredictions: 3, totalPredictions: 9, isCurrentUser: false },
    { rank: 10, displayName: 'Новичок42', photoUrl: null, totalPoints: 10, correctPredictions: 1, totalPredictions: 5, isCurrentUser: false },
  ],
  currentUser: { rank: 3, displayName: 'МафиозоДжо', photoUrl: null, totalPoints: 65, correctPredictions: 9, totalPredictions: 15, isCurrentUser: true },
};

export const demoStats: TournamentStats = {
  tournamentId: 1,
  totalMatches: 5,
  matchesByState: { Upcoming: 1, Open: 1, Locked: 1, Resolved: 2 },
  totalPredictions: 122,
};

/** Basic match info (no predictions, no voteStats) */
export const demoMatchInfos: MatchInfo[] = demoMatches.map(m => ({
  id: m.id,
  gameNumber: m.gameNumber,
  tableNumber: m.tableNumber,
  state: m.state,
}));

/** Predictions keyed by matchId */
export const demoPredictionsMap: PredictionsMap = Object.fromEntries(
  demoMatches
    .filter(m => m.myPrediction != null)
    .map(m => [String(m.id), m.myPrediction!])
);

// Demo blob states with match results for resolved matches
function makeBlobState(
  match: MatchDto,
  votes: { total: number; town: number; mafia: number; slots: { slot: number; count: number; percent: number }[] } | null,
  result: { winningSide: number; votedOutSlots: number[] } | null = null,
): BlobMatchState {
  return {
    matchId: match.id,
    tournamentId: 1,
    version: 1,
    state: MatchState[match.state],
    updatedAt: new Date().toISOString(),
    tableSize: 10,
    totalPredictions: votes?.total ?? 0,
    winnerVotes: votes ? {
      town: { count: Math.round(votes.total * votes.town / 100), percent: votes.town },
      mafia: { count: Math.round(votes.total * votes.mafia / 100), percent: votes.mafia },
    } : null,
    votedOutVotes: votes?.slots ?? null,
    matchResult: result,
  };
}

export const demoBlobStates: Record<number, BlobMatchState> = {
  1: makeBlobState(demoMatches[0], { total: 42, town: 62, mafia: 38, slots: [
    { slot: 1, count: 3, percent: 7 }, { slot: 2, count: 5, percent: 12 }, { slot: 3, count: 12, percent: 29 },
    { slot: 4, count: 2, percent: 5 }, { slot: 5, count: 6, percent: 14 }, { slot: 6, count: 1, percent: 2 },
    { slot: 7, count: 4, percent: 10 }, { slot: 8, count: 3, percent: 7 }, { slot: 9, count: 2, percent: 5 },
    { slot: 10, count: 4, percent: 10 },
  ]}, { winningSide: 0, votedOutSlots: [3, 7] }),
  2: makeBlobState(demoMatches[1], { total: 38, town: 71, mafia: 29, slots: [
    { slot: 1, count: 5, percent: 13 }, { slot: 2, count: 2, percent: 5 }, { slot: 3, count: 4, percent: 11 },
    { slot: 4, count: 7, percent: 18 }, { slot: 5, count: 3, percent: 8 }, { slot: 6, count: 6, percent: 16 },
    { slot: 7, count: 2, percent: 5 }, { slot: 8, count: 4, percent: 11 }, { slot: 9, count: 3, percent: 8 },
    { slot: 10, count: 2, percent: 5 },
  ]}, { winningSide: 0, votedOutSlots: [4] }),
  3: makeBlobState(demoMatches[2], { total: 15, town: 53, mafia: 47, slots: [
    { slot: 1, count: 2, percent: 13 }, { slot: 2, count: 1, percent: 7 }, { slot: 3, count: 3, percent: 20 },
    { slot: 4, count: 1, percent: 7 }, { slot: 5, count: 2, percent: 13 }, { slot: 6, count: 0, percent: 0 },
    { slot: 7, count: 2, percent: 13 }, { slot: 8, count: 1, percent: 7 }, { slot: 9, count: 1, percent: 7 },
    { slot: 10, count: 2, percent: 13 },
  ]}),
  5: makeBlobState(demoMatches[4], { total: 27, town: 44, mafia: 56, slots: [
    { slot: 1, count: 1, percent: 4 }, { slot: 2, count: 3, percent: 11 }, { slot: 3, count: 2, percent: 7 },
    { slot: 4, count: 5, percent: 19 }, { slot: 5, count: 4, percent: 15 }, { slot: 6, count: 2, percent: 7 },
    { slot: 7, count: 3, percent: 11 }, { slot: 8, count: 2, percent: 7 }, { slot: 9, count: 4, percent: 15 },
    { slot: 10, count: 1, percent: 4 },
  ]}),
};
