-- ============================================================
-- Mafia Pick'em Database Schema Migration
-- Version: 001
-- Date: 2026-02-26
-- Description: Initial schema creation with all tables and indices
-- ============================================================

USE [MafiaPickem];
GO

-- Drop existing tables in dependency order (child tables first)
DROP TABLE IF EXISTS pickem.PredictionScore;
DROP TABLE IF EXISTS pickem.Prediction;
DROP TABLE IF EXISTS pickem.MatchResult;
DROP TABLE IF EXISTS pickem.Match;
DROP TABLE IF EXISTS pickem.Leaderboard;
DROP TABLE IF EXISTS pickem.Tournament;
DROP TABLE IF EXISTS pickem.PickemUser;
GO

-- Drop and recreate schema
DROP SCHEMA IF EXISTS pickem;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'pickem')
    EXEC ('CREATE SCHEMA pickem');
GO

-- ============================================================
-- Пользователь Telegram
-- ============================================================
CREATE TABLE pickem.PickemUser
(
    Id              int identity    NOT NULL
        CONSTRAINT PK_PickemUser PRIMARY KEY,
    TelegramId      bigint          NOT NULL,
    GameNickname    nvarchar(100)   NOT NULL,
    PhotoUrl        nvarchar(500),
    DateCreated     datetime2(0)    DEFAULT sysdatetime() NOT NULL,

    CONSTRAINT UQ_PickemUser_TelegramId UNIQUE (TelegramId),
    CONSTRAINT UQ_PickemUser_GameNickname UNIQUE (GameNickname)
);
GO

-- ============================================================
-- Турнир Pick'em
-- ============================================================
CREATE TABLE pickem.Tournament
(
    Id                  int identity    NOT NULL
        CONSTRAINT PK_PickemTournament PRIMARY KEY,
    Name                nvarchar(300)   NOT NULL,
    Description         nvarchar(1000),
    ImageUrl            nvarchar(max),
    Active              bit             DEFAULT 1 NOT NULL,
    DateCreated         datetime2(0)    DEFAULT sysdatetime() NOT NULL
);
GO

-- ============================================================
-- Матч внутри Pick'em турнира
-- ============================================================
CREATE TABLE pickem.Match
(
    Id                  int identity    NOT NULL
        CONSTRAINT PK_PickemMatch PRIMARY KEY,
    TournamentId        int             NOT NULL
        CONSTRAINT FK_PickemMatch_Tournament
            REFERENCES pickem.Tournament (Id)
            ON DELETE CASCADE,
    ExternalMatchRef    nvarchar(100),
    GameNumber          int             NOT NULL,
    TableNumber         int,
    State               tinyint         DEFAULT 0 NOT NULL,
    DateCreated         datetime2(0)    DEFAULT sysdatetime() NOT NULL,
    DateOpened          datetime2(0),
    DateLocked          datetime2(0),
    DateResolved        datetime2(0)
);
GO

/*
    Match.State values:
    0 = Upcoming
    1 = Open
    2 = Locked
    3 = Resolved
    4 = Canceled
*/

CREATE INDEX IX_PickemMatch_TournamentId_State
    ON pickem.Match (TournamentId, State);
GO

-- ============================================================
-- Результат матча (правильные ответы, вносит модератор)
-- ============================================================
CREATE TABLE pickem.MatchResult
(
    Id                  int identity    NOT NULL
        CONSTRAINT PK_MatchResult PRIMARY KEY,
    MatchId             int             NOT NULL
        CONSTRAINT FK_MatchResult_Match
            REFERENCES pickem.Match (Id)
            ON DELETE CASCADE,
    WinningSide         tinyint         NOT NULL,
    CorrectVotedOutCsv  nvarchar(100)   NOT NULL,
    DateCreated         datetime2(0)    DEFAULT sysdatetime() NOT NULL,

    CONSTRAINT UQ_MatchResult_Match
        UNIQUE (MatchId)
);
GO

/*
    WinningSide values:
    0 = Мирные (Red/Town)
    1 = Мафия (Black/Mafia)

    CorrectVotedOutCsv values:
    Отсортированные номера слотов через запятую (например: "0" или "3,7").
    0 = Никто, 1..10 = Номер игрока.
*/

-- ============================================================
-- Прогноз пользователя на матч
-- ============================================================
CREATE TABLE pickem.Prediction
(
    Id                  int identity    NOT NULL
        CONSTRAINT PK_Prediction PRIMARY KEY,
    MatchId             int             NOT NULL
        CONSTRAINT FK_Prediction_Match
            REFERENCES pickem.Match (Id)
            ON DELETE CASCADE,
    UserId              int             NOT NULL
        CONSTRAINT FK_Prediction_User
            REFERENCES pickem.PickemUser (Id),
    PredictedWinner     tinyint         NOT NULL,
    PredictedVotedOut   tinyint         NOT NULL,
    DateCreated         datetime2(0)    DEFAULT sysdatetime() NOT NULL,
    DateUpdated         datetime2(0),

    CONSTRAINT UQ_Prediction_Match_User
        UNIQUE (MatchId, UserId)
);
GO

/*
    PredictedWinner values:
    0 = Мирные
    1 = Мафия

    PredictedVotedOut values:
    0 = Никто
    1..10 = Номер игрока
*/

CREATE INDEX IX_Prediction_MatchId
    ON pickem.Prediction (MatchId)
    INCLUDE (PredictedWinner, PredictedVotedOut);
GO

CREATE INDEX IX_Prediction_UserId
    ON pickem.Prediction (UserId);
GO

-- ============================================================
-- Начисленные очки за прогноз (после Resolved)
-- ============================================================
CREATE TABLE pickem.PredictionScore
(
    Id                  int identity    NOT NULL
        CONSTRAINT PK_PredictionScore PRIMARY KEY,
    PredictionId        int             NOT NULL
        CONSTRAINT FK_PredictionScore_Prediction
            REFERENCES pickem.Prediction (Id)
            ON DELETE CASCADE,
    WinnerPoints        decimal(12, 4)  DEFAULT 0 NOT NULL,
    VotedOutPoints      decimal(12, 4)  DEFAULT 0 NOT NULL,
    TotalPoints         decimal(12, 4)  DEFAULT 0 NOT NULL,
    TotalVotes          int             NOT NULL,
    CorrectWinnerVotes  int             NOT NULL,
    CorrectVotedOutVotes int            NOT NULL,
    DateCalculated      datetime2(0)    DEFAULT sysdatetime() NOT NULL,

    CONSTRAINT UQ_PredictionScore_Prediction
        UNIQUE (PredictionId)
);
GO

-- ============================================================
-- Таблица лидеров (агрегация по турниру)
-- ============================================================
CREATE TABLE pickem.Leaderboard
(
    Id                  int identity    NOT NULL
        CONSTRAINT PK_Leaderboard PRIMARY KEY,
    TournamentId        int             NOT NULL
        CONSTRAINT FK_Leaderboard_Tournament
            REFERENCES pickem.Tournament (Id)
            ON DELETE CASCADE,
    UserId              int             NOT NULL
        CONSTRAINT FK_Leaderboard_User
            REFERENCES pickem.PickemUser (Id),
    TotalPoints         decimal(12, 4)  DEFAULT 0 NOT NULL,
    CorrectPredictions  int             DEFAULT 0 NOT NULL,
    TotalPredictions    int             DEFAULT 0 NOT NULL,
    Rank                int,
    DateUpdated         datetime2(0)    DEFAULT sysdatetime() NOT NULL,

    CONSTRAINT UQ_Leaderboard_Tournament_User
        UNIQUE (TournamentId, UserId)
);
GO

CREATE INDEX IX_Leaderboard_TournamentId_TotalPoints
    ON pickem.Leaderboard (TournamentId, TotalPoints DESC);
GO

-- ============================================================
-- Migration complete
-- ============================================================
PRINT 'Schema pickem created successfully with all tables and indices';
GO
