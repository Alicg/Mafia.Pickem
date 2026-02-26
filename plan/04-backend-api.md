# Pick'em — Backend API

## Стек бэкенда

**Зафиксированный стек:** Azure Functions (.NET 8, Isolated Worker) + Azure Blob Storage (state JSON polling)

Подход укладывается в бюджет без SignalR и ориентирован на простой операционный контур.

## API Endpoints

### Публичные (для Mini App)

| Method | Endpoint | Описание | Auth |
|--------|----------|----------|------|
| POST | `/api/auth/telegram` | Валидация initData, возврат session token | Telegram initData |
| GET | `/api/tournaments/active` | Список активных турниров Pick'em | Token |
| GET | `/api/tournaments/{id}` | Детали турнира + текущий матч | Token |
| GET | `/api/tournaments/{id}/matches` | Все матчи турнира | Token |
| GET | `/api/matches/{id}` | Детали матча + свой прогноз + статистика (если Open/Locked) | Token |
| POST | `/api/matches/{id}/predict` | Сохранить/обновить прогноз | Token + RegisteredUser |
| GET | `/api/tournaments/{id}/leaderboard` | Таблица лидеров | Token |
| GET | `/api/me` | Профиль текущего пользователя + статус регистрации | Token |
| POST | `/api/me/nickname` | Установить/обновить игровой ник | Token |
| GET | `/api/matches/{id}/state-meta` | Мета состояния (version, updatedAt, blobUrl) | Token |

### Админские (для модератора)

| Method | Endpoint | Описание | Auth |
|--------|----------|----------|------|
| POST | `/api/admin/matches` | Создать новый матч | Admin |
| POST | `/api/admin/matches/{id}/open` | Перевести матч в Open | Admin |
| POST | `/api/admin/matches/{id}/lock` | Перевести матч в Locked | Admin |
| POST | `/api/admin/matches/{id}/resolve` | Завершить матч + начислить очки | Admin |
| POST | `/api/admin/matches/{id}/cancel` | Аннулировать матч | Admin |
| GET | `/api/admin/tournaments/{id}/stats` | Статистика участия | Admin |
| POST | `/api/admin/matches/{id}/publish-state` | Принудительная публикация `match-state-<id>.json` | Admin |

### Служебные (бот)

| Method | Endpoint | Описание | Auth |
|--------|----------|----------|------|
| POST | `/api/bot/webhook` | Telegram bot updates (служебные события бота) | Telegram secret |

## Модели (DTOs)

### Request Models

```csharp
// Аутентификация
public class TelegramAuthRequest
{
    public string? InitData { get; set; }
}

public class UpdateNicknameRequest
{
  public string? GameNickname { get; set; }
}

// Прогноз
public class SubmitPredictionRequest
{
    public int? MatchId { get; set; }
    public byte? PredictedWinner { get; set; }  // 0 = Мирные, 1 = Мафия
    public byte? PredictedVotedOut { get; set; } // 0 = Никто, 1-10 = Игрок
}

// Админ: Завершить матч
public class ResolveMatchRequest
{
    public int? MatchId { get; set; }
    public byte? WinningSide { get; set; }        // 0 = Мирные, 1 = Мафия
    public List<byte>? VotedOutSlots { get; set; } // [0] = Никто, [3, 7] = Игроки 3 и 7; перед сохранением сортируется и сериализуется в CSV
}
```

### Response Models

```csharp
public class TournamentDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public MatchDto? CurrentMatch { get; set; }
}

public class MatchDto
{
    public int? Id { get; set; }
    public int? GameNumber { get; set; }
    public int? TableNumber { get; set; }
    public byte? State { get; set; }            // 0-4
    public PredictionDto? MyPrediction { get; set; }
    public VoteStatsDto? VoteStats { get; set; } // null если State < Open
}

public class PredictionDto
{
    public byte? PredictedWinner { get; set; }
    public byte? PredictedVotedOut { get; set; }
    public decimal? WinnerPoints { get; set; }   // null если не Resolved
    public decimal? VotedOutPoints { get; set; }
    public decimal? TotalPoints { get; set; }
}

public class VoteStatsDto
{
    public int? TotalVotes { get; set; }
    public decimal? TownPercentage { get; set; }
    public decimal? MafiaPercentage { get; set; }
    public List<SlotVoteDto>? SlotVotes { get; set; }
}

public class SlotVoteDto
{
    public byte? Slot { get; set; }
    public decimal? Percentage { get; set; }
}

public class LeaderboardEntryDto
{
    public int? Rank { get; set; }
    public string? DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? TotalPoints { get; set; }
    public int? CorrectPredictions { get; set; }
    public int? TotalPredictions { get; set; }
    public bool? IsCurrentUser { get; set; }
}
```

## Blob State Publishing (сервер → клиент через polling)

- Файл в Blob: `match-state-<id>.json`
- Перезаписывается при изменениях матча или прогнозов
- Клиент опрашивает файл с интервалом 2-20 секунд в зависимости от состояния

## Бизнес-логика (Services)

### MatchStateService

Отвечает за переходы состояний матча и побочные эффекты:

```
Open:
  - Валидация: State == Upcoming
  - Действие: UPDATE Match SET State = 1, DateOpened = NOW
  - Side effect: публикация `match-state-<id>.json`

Locked:
  - Валидация: State == Open
  - Действие: UPDATE Match SET State = 2, DateLocked = NOW
  - Side effect: Рассчитать VoteStats (% голосов)
  - Side effect: публикация `match-state-<id>.json`

Resolved:
  - Валидация: State == Locked
  - Действие:
    1. INSERT MatchResult (WinningSide, CorrectVotedOutCsv)
    2. VotedOutSlots сортируются и сохраняются строкой CSV (например, `3,7`)
    3. Рассчитать PredictionScore для каждого прогноза
    4. UPDATE/MERGE Leaderboard
    5. UPDATE Match SET State = 3, DateResolved = NOW
  - Side effect: публикация `match-state-<id>.json`

Canceled:
  - Валидация: State IN (Upcoming, Open, Locked)
  - Действие: UPDATE Match SET State = 4
  - Side effect: DELETE все PredictionScore для этого матча (если были)
  - Side effect: публикация `match-state-<id>.json`
```

### PredictionService

```
SubmitPrediction:
  - Валидация:
    1. Match.State == Open
    2. User имеет заполненный `GameNickname`
    3. PredictedWinner IN (0, 1)
    4. PredictedVotedOut IN (0..10)
  - Действие: MERGE Prediction (upsert)
  - Side effect: публикация `match-state-<id>.json` по политике (раз в 10 сек в Open, принудительно при переходе в Locked)
  - Ответ: 200 OK
```

### ScoringService

```
CalculateScores(matchId):
  Формула из PRD:
    WinnerPoints = (TotalVotes / CorrectWinnerVotes) * 10
    VotedOutPoints = (TotalVotes / CorrectVotedOutVotes) * 20

  Защита от деления на ноль:
    Если CorrectVotes == 0 → Points = 0
```

## Авторизация и Middleware

### Telegram Token Auth

Каждый запрос от Mini App содержит заголовок:
```
Authorization: TelegramMiniApp <initData>
```

Middleware (или Azure Functions filter):
1. Извлечь `initData` из заголовка
2. Валидировать HMAC-SHA256 (см. 03-telegram-mini-app.md)
3. Извлечь `telegram_user_id`
4. Найти/создать `pickem.PickemUser`
5. Установить UserId в контексте запроса

### Admin Auth

Список Telegram ID администраторов хранится в конфигурации (App Settings):
```json
{
  "PickemAdminTelegramIds": [123456789, 987654321]
}
```

## Структура проекта Azure Functions

```
MafiaPickem.Api/
├── Program.cs                      # DI, Dapper, Blob, Telegram bot client
├── host.json
├── Functions/
│   ├── AuthFunction.cs             # POST /api/auth/telegram
│   ├── ProfileFunction.cs          # GET /api/me, POST /api/me/nickname
│   ├── TournamentFunctions.cs      # GET tournaments
│   ├── MatchFunctions.cs           # GET matches, POST predict
│   ├── LeaderboardFunction.cs      # GET leaderboard
│   ├── AdminFunctions.cs           # POST open/lock/resolve/cancel
│   └── BotWebhookFunction.cs       # POST /api/bot/webhook
├── Services/
│   ├── MatchStateService.cs
│   ├── PredictionService.cs
│   ├── ScoringService.cs
│   ├── StatePublishService.cs
│   ├── TelegramAuthService.cs
│   └── NicknameService.cs
├── Data/
│   ├── PickemUserRepository.cs
│   ├── TournamentRepository.cs
│   ├── MatchRepository.cs
│   ├── PredictionRepository.cs
│   └── LeaderboardRepository.cs
├── Auth/
│   └── TelegramAuthMiddleware.cs
├── Bot/
│   ├── TelegramWebhookValidator.cs
│   └── TelegramBotClient.cs
├── State/
│   ├── MatchStateDto.cs
│   └── MatchStateBlobWriter.cs
└── Models/
    ├── Requests/
    └── Responses/
```
