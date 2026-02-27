import {
  UserProfile,
  TournamentDto,
  MatchDto,
  MatchState,
  LeaderboardResponse,
  TournamentStats,
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
    voteStats: {
      totalVotes: 42,
      townPercentage: 62,
      mafiaPercentage: 38,
      slotVotes: [
        { slot: 1, count: 3, percentage: 7 },
        { slot: 2, count: 5, percentage: 12 },
        { slot: 3, count: 12, percentage: 29 },
        { slot: 4, count: 2, percentage: 5 },
        { slot: 5, count: 6, percentage: 14 },
        { slot: 6, count: 1, percentage: 2 },
        { slot: 7, count: 4, percentage: 10 },
        { slot: 8, count: 3, percentage: 7 },
        { slot: 9, count: 2, percentage: 5 },
        { slot: 10, count: 4, percentage: 10 },
      ],
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
    voteStats: {
      totalVotes: 38,
      townPercentage: 71,
      mafiaPercentage: 29,
      slotVotes: [
        { slot: 1, count: 5, percentage: 13 },
        { slot: 2, count: 2, percentage: 5 },
        { slot: 3, count: 4, percentage: 11 },
        { slot: 4, count: 7, percentage: 18 },
        { slot: 5, count: 3, percentage: 8 },
        { slot: 6, count: 6, percentage: 16 },
        { slot: 7, count: 2, percentage: 5 },
        { slot: 8, count: 4, percentage: 11 },
        { slot: 9, count: 3, percentage: 8 },
        { slot: 10, count: 2, percentage: 5 },
      ],
    },
  },
  {
    id: 3,
    gameNumber: 3,
    tableNumber: 2,
    state: MatchState.Open,
    myPrediction: null,
    voteStats: {
      totalVotes: 15,
      townPercentage: 53,
      mafiaPercentage: 47,
      slotVotes: [
        { slot: 1, count: 2, percentage: 13 },
        { slot: 2, count: 1, percentage: 7 },
        { slot: 3, count: 3, percentage: 20 },
        { slot: 4, count: 1, percentage: 7 },
        { slot: 5, count: 2, percentage: 13 },
        { slot: 6, count: 0, percentage: 0 },
        { slot: 7, count: 2, percentage: 13 },
        { slot: 8, count: 1, percentage: 7 },
        { slot: 9, count: 1, percentage: 7 },
        { slot: 10, count: 2, percentage: 13 },
      ],
    },
  },
  {
    id: 4,
    gameNumber: 4,
    tableNumber: 2,
    state: MatchState.Upcoming,
    myPrediction: null,
    voteStats: null,
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
    voteStats: {
      totalVotes: 27,
      townPercentage: 44,
      mafiaPercentage: 56,
      slotVotes: [
        { slot: 1, count: 1, percentage: 4 },
        { slot: 2, count: 3, percentage: 11 },
        { slot: 3, count: 2, percentage: 7 },
        { slot: 4, count: 5, percentage: 19 },
        { slot: 5, count: 4, percentage: 15 },
        { slot: 6, count: 2, percentage: 7 },
        { slot: 7, count: 3, percentage: 11 },
        { slot: 8, count: 2, percentage: 7 },
        { slot: 9, count: 4, percentage: 15 },
        { slot: 10, count: 1, percentage: 4 },
      ],
    },
  },
];

export const demoTournament: TournamentDto = {
  id: 1,
  name: 'Мафия Кубок Зимы 2026',
  description: 'Зимний турнир по мафии — сделай свой прогноз!',
  imageUrl: null,
  currentMatch: demoMatches[2], // Game #3 (Open)
};

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
