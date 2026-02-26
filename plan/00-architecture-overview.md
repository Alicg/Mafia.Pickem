# Pick'em PRD (Product Requirements Document)

## 1. Product Summary

**Product name:** Mafia Pick'em  
**Platform:** Telegram Mini App (mobile-first)  
**Goal:** Повысить вовлеченность зрителей трансляций через интерактив прогнозов с рейтингом.

Пользователи делают прогнозы до начала активной фазы игры, получают очки по динамической формуле и соревнуются в лидерборде турнира.

## 2. Business Goals And Success Metrics

### Business goals

1. Увеличить удержание зрителей в течение трансляции.
2. Обеспечить открытый доступ к интерактиву для всех пользователей.
3. Запустить MVP с бюджетом инфраструктуры до `$10/месяц`.

### Product KPIs

1. `DAU/WAU` в Mini App.
2. Доля пользователей, завершивших регистрацию (заполнили игровой ник).
3. Среднее число матчей с прогнозом на пользователя за турнир.
4. Доля пользователей, вернувшихся в Mini App после push от бота.
5. P95 latency обновления UI после смены статуса матча (целевой диапазон: `1-3 сек`).

## 3. Users And Roles

1. `Viewer` — неавторизованный, доступен просмотр.
2. `Registered User` — указал игровой ник, может делать прогнозы и участвовать в лидерборде.
3. `Moderator` — управляет состояниями матчей и фиксирует результаты.
4. `Admin` — доступ к конфигурации турнира и операционным действиям.

## 4. Core User Flows

### 4.1 Viewer to Player

1. Пользователь открывает бота и нажимает `Открыть Pick'em`.
2. Mini App запускается и проходит Telegram auth.
3. Если игровой ник не заполнен, пользователь проходит регистрацию.
4. Пользователь указывает только игровой ник и сохраняет его.
5. После регистрации пользователь может делать прогнозы.

### 4.2 Match participation

1. Матч в статусе `Open`.
2. Пользователь выбирает:
    - победителя (`Мирные`/`Мафия`)
    - кто уйдет на первом голосовании (`1..N` или `Никто`)
3. Нажимает `Сохранить прогноз`.
4. После `Locked` видит агрегированную статистику толпы.
5. После `Resolved` получает очки и обновленное место в лидерборде.

### 4.3 Moderator workflow

1. Создает/активирует матч.
2. Переводит статусы: `Upcoming -> Open -> Locked -> Resolved`.
3. Вводит правильные ответы и завершает матч.
4. При необходимости аннулирует матч (`Canceled`).

## 5. Functional Requirements

### 5.1 Match state machine

1. `Upcoming` — отображение карточки, прогноз недоступен.
2. `Open` — прием/изменение прогнозов разрешен, показывается crowd-статистика.
3. `Locked` — прогнозы заблокированы, показывается финальная crowd-статистика перед `Resolved`.
4. `Resolved` — результаты зафиксированы, очки начислены, показывается итоговая crowd-статистика.
5. `Canceled` — матч отменен, очки за матч не начисляются.

### 5.2 Predictions

1. Один пользователь имеет один активный прогноз на матч (upsert).
2. Прогноз можно менять только в `Open`.
3. В `Locked/Resolved/Canceled` изменения запрещены.

### 5.3 Scoring model

`WinnerPoints = (TotalVotes / CorrectWinnerVotes) * 10`  
`VotedOutPoints = (TotalVotes / CorrectVotedOutVotes) * 20`  
`TotalPoints = WinnerPoints + VotedOutPoints`

Если знаменатель равен `0`, соответствующие очки равны `0`.

### 5.4 Leaderboard

1. Ранжирование по `TotalPoints`.
2. Обновление после `Resolved`.
3. В UI показывать Top + текущую позицию пользователя.

### 5.5 Registration

1. Доступ к прогнозам бесплатный для всех зарегистрированных пользователей.
2. При первой авторизации пользователь обязан указать только `GameNickname`.
3. До сохранения `GameNickname` отправка прогноза запрещена.

## 6. Non-Functional Requirements

1. Mobile-first UX в Telegram WebView.
2. Время первой загрузки экрана матча: целевой P95 `< 2.5 сек`.
3. Обновление состояния через polling с целевым lag `1-3 сек`.
4. Доступность API в период турнира: целевой SLO `99.5%`.
5. Наблюдаемость: логи API, ошибки webhook, время публикации state, метрики polling.

## 7. Technology Stack

1. **Frontend:** React + Vite + TypeScript + Telegram Web App SDK.
2. **Backend:** Azure Functions (.NET 8 Isolated Worker).
3. **Database:** Azure SQL (`pickem` schema).
4. **State distribution:** Azure Blob Storage, файл `match-state-<id>.json`.
5. **Hosting:** Static hosting for Mini App + Azure Functions for API/bot webhook.

## 8. System Architecture

```
Telegram Bot
    │
    ├── launches Mini App
    └── forwards user to Mini App entrypoint

Mini App (React)
    ├── calls API (auth, predict, leaderboard, admin)
    └── polls Blob: match-state-<id>.json

Azure Functions
    ├── validates Telegram initData
    ├── handles nickname registration
    ├── handles business operations
    └── publishes aggregated match-state JSON to Blob

Azure SQL
    └── source of truth for users, matches, predictions, scores
```

## 9. Data Contracts (High-Level)

### 9.1 Public state file

`match-state-<id>.json` содержит только агрегаты (без персональных данных):

1. `matchId`, `tournamentId`, `version`, `state`, `updatedAt`.
2. `totalPredictions`.
3. `winnerVotes` (`town`/`mafia`, count + percent).
4. `votedOutVotes` (slot -> count + percent).

### 9.2 Cache policy

`Cache-Control` для state-файла: `max-age=1, must-revalidate`.

## 10. Security And Compliance

1. Telegram auth: валидация `initData` через HMAC-SHA256 и Bot Token.
2. Admin доступ: allowlist `TelegramUserId` в конфиге.
3. Webhook security: secret token в заголовке + валидация источника.
4. В публичном state-файле запрещены user-level данные.

## 11. Release Plan

### Phase 0: Foundation (1-2 days)

1. Бот и webhook.
2. React skeleton + Telegram SDK bootstrap.
3. Azure Functions skeleton.
4. SQL schema migration.

### Phase 1: Core Gameplay (3-4 days)

1. Auth, турниры, матчи, прогнозы.
2. State publisher -> Blob.
3. Polling client in Mini App.

### Phase 2: Moderator And Scoring (2-3 days)

1. Admin actions (open/lock/resolve/cancel).
2. Scoring and leaderboard pipeline.
3. End-to-end checks for match lifecycle.

### Phase 3: Registration And Profile (1-2 days)

1. Registration endpoint для установки `GameNickname`.
2. UI-экран регистрации (только игровой ник).
3. Ограничение прогнозов до завершения регистрации.

### Phase 4: Stabilization (2-3 days)

1. Load tests for 100 concurrent users.
2. Monitoring and alerting baselines.
3. Edge cases and tournament rehearsal.

## 12. Risks And Mitigations

1. **Polling lag during peak load** -> adaptive intervals + lightweight JSON + throttled publishing (1 раз/10 сек в `Open`).
2. **Invalid nickname input / duplicates** -> серверная валидация формата и уникальности ника.
3. **Telegram WebView quirks** -> device matrix tests (iOS/Android).
4. **Operator mistakes in admin flow** -> role checks + confirmation dialogs + audit log.

## 13. Acceptance Criteria (MVP)

1. Пользователь может пройти путь: открыть Mini App -> указать игровой ник -> сделать прогноз.
2. Модератор может пройти полный lifecycle матча.
3. После `Resolved` очки и лидерборд обновляются корректно.
4. UI у 100 одновременных пользователей обновляется в пределах целевого lag.
5. Регистрация по игровому нику валидируется и не позволяет пустые/дублирующиеся значения.

## 14. Deferred Items

1. Финальные домены Mini App и API для `dev/prod` (для настройки BotFather и webhook URL).

## 15. Companion Documents

1. `01-realtime-transport.md` - details polling and Blob state strategy.
2. `02-database-schema.md` - SQL schema and queries.
3. `03-telegram-mini-app.md` - Telegram UX and registration flow details.
4. `04-backend-api.md` - API and service-level design.
5. `05-implementation-roadmap.md` - execution checklist.
