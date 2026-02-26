# Pick'em — Схема базы данных

## Принципы

- Новая схема `pickem` в готовой Azure SQL Database
- Полная автономность: только таблицы схемы `pickem`
- Без FK/JOIN на таблицы из других схем
- Индексы на горячие таблицы (Prediction — тысячи одновременных записей)
- Схема ориентирована на быстрый upsert прогнозов и пересчет лидерборда

## ER-диаграмма (упрощённая)

```
pickem.PickemUser ────────────────────────────┐
    │                                      │
    │                                      │
    ▼                                      ▼
pickem.Tournament ◄──── pickem.Match ──► pickem.Prediction
       │                    │                  │
       │                    │                  │
       │                    ▼                  │
    │            pickem.MatchResult         │
       │                                       │
       └──────► pickem.Leaderboard ◄──────────┘
```

## Маппинг сущностей PRD → таблицы

| PRD-сущность | Таблица | Назначение |
|-------------|---------|------------|
| Турнир Pick'em | `pickem.Tournament` | Конфигурация турнира внутри Mini App |
| Match (игра) | `pickem.Match` | Каждая игра с состоянием (Upcoming/Open/Locked/Resolved/Canceled) |
| Прогноз | `pickem.Prediction` | Выбор пользователя на конкретный матч |
| Правильные ответы | `pickem.MatchResult` | Результаты матча (кто победил + кто ушел на первом голосовании) |
| Leaderboard | `pickem.Leaderboard` | Агрегированные очки пользователей по турниру |
| Очки за матч | `pickem.PredictionScore` | Детализация начисленных очков |
| Пользователь | `pickem.PickemUser` | Telegram-пользователь |

## SQL-скрипт

```sql
USE [YourDatabaseName];
GO

-- Drop tables in dependency order (child tables first)
DROP TABLE IF EXISTS pickem.PredictionScore;
DROP TABLE IF EXISTS pickem.Prediction;
DROP TABLE IF EXISTS pickem.MatchResult;
DROP TABLE IF EXISTS pickem.Match;
DROP TABLE IF EXISTS pickem.Leaderboard;
DROP TABLE IF EXISTS pickem.Tournament;
DROP TABLE IF EXISTS pickem.PickemUser;
GO

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
```

## Ключевые SQL-операции

### 1. Сохранение прогноза (MERGE — идемпотентный upsert)

```sql
MERGE pickem.Prediction AS target
USING (SELECT @MatchId, @UserId, @PredictedWinner, @PredictedVotedOut)
    AS source (MatchId, UserId, PredictedWinner, PredictedVotedOut)
ON target.MatchId = source.MatchId AND target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET 
        PredictedWinner = source.PredictedWinner,
        PredictedVotedOut = source.PredictedVotedOut,
        DateUpdated = sysdatetime()
WHEN NOT MATCHED THEN
    INSERT (MatchId, UserId, PredictedWinner, PredictedVotedOut)
    VALUES (source.MatchId, source.UserId, source.PredictedWinner, source.PredictedVotedOut);
```

### 2. Расчёт очков (при Resolved)

```sql
-- Подсчёт голосов по матчу
DECLARE @TotalVotes int, @CorrectWinner int, @CorrectVotedOut int;

SELECT @TotalVotes = COUNT(*)
FROM pickem.Prediction WHERE MatchId = @MatchId;

SELECT @CorrectWinner = COUNT(*)
FROM pickem.Prediction p
INNER JOIN pickem.MatchResult mr ON mr.MatchId = p.MatchId
WHERE p.MatchId = @MatchId AND p.PredictedWinner = mr.WinningSide;

SELECT @CorrectVotedOut = COUNT(*)
FROM pickem.Prediction p
INNER JOIN pickem.MatchResult mr ON mr.MatchId = p.MatchId
WHERE p.MatchId = @MatchId 
  AND EXISTS (
        SELECT 1
        FROM STRING_SPLIT(mr.CorrectVotedOutCsv, ',') cv
        WHERE TRY_CAST(cv.value AS tinyint) = p.PredictedVotedOut
  );

-- Начисление очков каждому прогнозу
INSERT INTO pickem.PredictionScore (PredictionId, WinnerPoints, VotedOutPoints, TotalPoints,
                              TotalVotes, CorrectWinnerVotes, CorrectVotedOutVotes)
SELECT 
    p.Id,
    CASE WHEN p.PredictedWinner = mr.WinningSide AND @CorrectWinner > 0
         THEN CAST(@TotalVotes AS decimal) / @CorrectWinner * 10
         ELSE 0 END,
        CASE WHEN EXISTS (
                      SELECT 1
                      FROM STRING_SPLIT(mr.CorrectVotedOutCsv, ',') cv
                      WHERE TRY_CAST(cv.value AS tinyint) = p.PredictedVotedOut
                  )
              AND @CorrectVotedOut > 0
         THEN CAST(@TotalVotes AS decimal) / @CorrectVotedOut * 20
         ELSE 0 END,
    -- TotalPoints = WinnerPoints + VotedOutPoints (computed in app layer)
    0,
    @TotalVotes,
    @CorrectWinner,
    @CorrectVotedOut
FROM pickem.Prediction p
INNER JOIN pickem.MatchResult mr ON mr.MatchId = p.MatchId
WHERE p.MatchId = @MatchId;

-- Обновить TotalPoints
UPDATE pickem.PredictionScore
SET TotalPoints = WinnerPoints + VotedOutPoints
WHERE PredictionId IN (SELECT Id FROM pickem.Prediction WHERE MatchId = @MatchId);
```

### 3. Обновление Leaderboard (после расчёта очков)

```sql
MERGE pickem.Leaderboard AS target
USING (
    SELECT 
        @TournamentId AS TournamentId,
        p.UserId,
        SUM(ps.TotalPoints) AS TotalPoints,
        SUM(CASE WHEN ps.TotalPoints > 0 THEN 1 ELSE 0 END) AS CorrectPredictions,
        COUNT(*) AS TotalPredictions
    FROM pickem.Prediction p
    INNER JOIN pickem.PredictionScore ps ON ps.PredictionId = p.Id
    INNER JOIN pickem.Match m ON m.Id = p.MatchId
    WHERE m.TournamentId = @TournamentId
    GROUP BY p.UserId
) AS source
ON target.TournamentId = source.TournamentId AND target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET 
        TotalPoints = source.TotalPoints,
        CorrectPredictions = source.CorrectPredictions,
        TotalPredictions = source.TotalPredictions,
        DateUpdated = sysdatetime()
WHEN NOT MATCHED THEN
    INSERT (TournamentId, UserId, TotalPoints, CorrectPredictions, TotalPredictions)
    VALUES (source.TournamentId, source.UserId, source.TotalPoints, source.CorrectPredictions, source.TotalPredictions);

-- Пересчитать ранги
;WITH Ranked AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY TotalPoints DESC) AS NewRank
    FROM pickem.Leaderboard
    WHERE TournamentId = @TournamentId
)
UPDATE l SET Rank = r.NewRank
FROM pickem.Leaderboard l
INNER JOIN Ranked r ON r.Id = l.Id;
```

### 4. Статистика голосов для Locked-состояния

```sql
SELECT 
    PredictedWinner,
    COUNT(*) AS VoteCount,
    CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS decimal(5,1)) AS Percentage
FROM pickem.Prediction
WHERE MatchId = @MatchId
GROUP BY PredictedWinner;

SELECT 
    PredictedVotedOut,
    COUNT(*) AS VoteCount,
    CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS decimal(5,1)) AS Percentage
FROM pickem.Prediction
WHERE MatchId = @MatchId
GROUP BY PredictedVotedOut;

```
