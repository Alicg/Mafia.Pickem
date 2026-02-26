# Pick'em — Real-time транспорт: выбранная реализация

## Зафиксированный выбор

- Frontend: **React**
- Backend: **Azure Functions**
- Real-time: **Polling одного файла `match-state-<id>.json` в Azure Blob Storage**
- Доступ: **полностью бесплатный**
- Регистрация: **обязательный игровой ник (`GameNickname`)**
- Бюджетный лимит: **до $10/месяц**

## Почему не SignalR/WebSocket

- Azure SignalR Standard ($49/мес) выше целевого бюджета.
- Free tier (20 concurrent) не покрывает сценарий до 100 одновременных пользователей.
- Циклическое переподключение пользователей ухудшит UX и приведет к потере событий.

## Как работает выбранный polling-подход

```
[Telegram Mini App] ── GET /blob/match-state-<id>.json (каждые 2-3 сек)
             │
             ├── если version не изменилась → UI без изменений
             └── если version изменилась   → UI обновляется
       
[Azure Functions]
    ├── POST /predict
    ├── POST /admin/open|lock|resolve|cancel
    └── после изменения состояния/прогнозов:
            пересчитывает агрегаты и перезаписывает Blob JSON
```

## Формат `match-state-<id>.json`

```json
{
    "matchId": 77,
    "tournamentId": 12,
    "version": 154,
    "state": "Locked",
    "updatedAt": "2026-02-26T18:41:30Z",
    "tableSize": 9,
    "totalPredictions": 83,
    "winnerVotes": {
        "town": { "count": 52, "percent": 62.7 },
        "mafia": { "count": 31, "percent": 37.3 }
    },
    "votedOutVotes": [
        { "slot": 0, "count": 6, "percent": 7.2 },
        { "slot": 1, "count": 9, "percent": 10.8 },
        { "slot": 2, "count": 7, "percent": 8.4 },
        { "slot": 3, "count": 14, "percent": 16.9 },
        { "slot": 4, "count": 8, "percent": 9.6 },
        { "slot": 5, "count": 10, "percent": 12.0 },
        { "slot": 6, "count": 11, "percent": 13.3 },
        { "slot": 7, "count": 5, "percent": 6.0 },
        { "slot": 8, "count": 7, "percent": 8.4 },
        { "slot": 9, "count": 6, "percent": 7.2 }
    ]
}
```

`slot: 0` = "Никто".

## Частота polling

| Match state | Интервал polling |
|------------|------------------|
| Upcoming | 10 сек |
| Open | 2 сек |
| Locked | 2 сек |
| Resolved | 15 сек |
| Canceled | 20 сек |

Если вкладка неактивна (`document.hidden = true`) — увеличить интервал до 20-30 сек.

## Cache-Control для Blob JSON

Для одного файла `match-state-<id>.json`:

- `Cache-Control: max-age=1, must-revalidate`
- `Content-Type: application/json`

Это снижает задержку и не дает кэшу отдавать устаревшее состояние надолго.

## Алгоритм обновления state в Functions

1. Любое изменение (`/predict`, `/admin/open`, `/admin/lock`, `/admin/resolve`, `/admin/cancel`) пишет данные в SQL.
2. Функция пересчитывает агрегаты для матча (`winnerVotes`, `votedOutVotes`).
3. Увеличивает `version`.
4. Перезаписывает `match-state-<id>.json` в Blob.

Политика публикации:

- В статусе `Open` публиковать `match-state-<id>.json` не чаще 1 раза в 10 секунд.
- При переходе матча в `Locked` выполнять принудительную публикацию один раз.
- Для `Resolved`/`Canceled` выполнять публикацию сразу после изменения статуса.

## Пример клиента (React)

```typescript
let timerId: number | null = null;
let lastVersion = 0;

async function pollMatchState(matchId: number, intervalMs: number): Promise<void> {
    const response = await fetch(`${BLOB_BASE_URL}/match-state-${matchId}.json`, {
        cache: "no-store"
    });
    if (!response.ok) {
        return;
    }

    const state = await response.json() as MatchStateDto;
    if (state.version > lastVersion) {
        lastVersion = state.version;
        renderMatchState(state);
    }
}

function startPolling(matchId: number): void {
    const intervalMs = getPollingIntervalByState(currentState);
    timerId = window.setInterval(() => {
        void pollMatchState(matchId, intervalMs);
    }, intervalMs);
}
```

## Стоимость (оценка)

| Сервис | Стоимость |
|--------|-----------|
| Azure Functions (Consumption) | ~$0 (в пределах free grants) |
| Azure Blob Storage (JSON state) | ~$0-2 |
| Azure Static Web Apps | $0 |
| Итого (без Front Door/CDN) | ~$0-2/мес |

## Ограничения подхода

- Нет субсекундного real-time для всех, реальная задержка 1-3 сек.
- Нужен аккуратный контроль кэша и интервалов polling.
- При резком росте аудитории может потребоваться Front Door/CDN или переход на push-модель.

## Когда пересматривать архитектуру

- 300+ одновременных пользователей в Open/Locked состоянии.
- Нужна гарантированная задержка < 1 сек.
- Возникает заметная нагрузка на Blob/Functions при текущих интервалах.

В этом случае следующая ступень: `Azure Front Door + Blob` или `SignalR/Web PubSub`.

