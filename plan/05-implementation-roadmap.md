# Pick'em — Роадмап реализации

## Зафиксированные решения

- Frontend: **React + Vite + TypeScript**
- Backend: **Azure Functions (.NET 8, Isolated Worker)**
- Real-time: **Polling одного Blob файла `match-state-<id>.json`**
- Access: **Бесплатный для всех зарегистрированных пользователей**
- Registration: **Только игровой ник (`GameNickname`)**

## Зафиксированные решения по реализации

1. Политика публикации state: в `Open` публиковать `match-state-<id>.json` не чаще 1 раза в 10 секунд; при переходе в `Locked` публиковать один раз сразу.
2. Контракт `match-state-<id>.json`: использовать агрегаты `winnerVotes` и `votedOutVotes` и сохранить текущий формат version/percent.
3. Доступ к Blob: публичный read на агрегированные state-файлы.
4. Telegram Bot webhook: `POST /api/bot/webhook` + секретный токен в заголовке + валидация источника + идемпотентная обработка update/event.
5. Доступ к прогнозам бесплатный; ограничение доступа только по завершённой регистрации (задан `GameNickname`).
6. Admin policy: список Telegram ID хранится в конфиге Azure Functions.
7. Crowd-статистика показывается в `Open`, `Locked` и `Resolved`.

## Отложено на потом

1. Финальные домены Mini App/API для `dev/prod` и настройки BotFather.

## Фазы

Метки выполнения:
- `[MANUAL]` — шаг должен быть выполнен вручную человеком. Для ручных шагов нужно сообщать детальное описание, кто именно и как должен его выполнить (например, "создать бота через @BotFather" или "настроить секреты в Azure Portal").
- `[AGENT]` — шаг может быть выполнен агентом самостоятельно (код, структура проекта, конфигурация в репозитории).

### Фаза 0: Инфраструктура (1-2 дня)

- [ ] [MANUAL] Создать Telegram бота через @BotFather
- [ ] [MANUAL] Создать репозиторий `MafiaPickem`
- [ ] [AGENT] Инициализировать Azure Functions проект (.NET 8, Isolated Worker)
- [ ] [AGENT] Инициализировать фронтенд-проект (React/Vite)
- [ ] [MANUAL] Настроить Azure Blob Storage контейнер для state JSON
- [ ] [MANUAL] Применить SQL-скрипт `pickem` на Azure SQL
- [ ] [AGENT] Настроить CI/CD (GitHub Actions → Azure)

### Фаза 1: Бэкенд — Ядро (3-4 дня)

- [ ] [AGENT] `TelegramAuthService` — валидация initData, выдача session token
- [ ] [AGENT] `PickemUserRepository` — CRUD для пользователей (upsert по TelegramId + хранение `GameNickname`)
- [ ] [AGENT] `TournamentRepository` — получение активных турниров
- [ ] [AGENT] `MatchRepository` — CRUD для матчей, переходы состояний
- [ ] [AGENT] `PredictionRepository` — upsert прогнозов
- [ ] [AGENT] `ScoringService` — расчёт очков по формуле из PRD
- [ ] [AGENT] `LeaderboardRepository` — агрегация и ранжирование
- [ ] [AGENT] `StatePublishService` — публикация `match-state-<id>.json` в Blob
- [ ] [AGENT] Публикация state по политике: раз в 10 сек в `Open` + принудительная публикация при переходе в `Locked`
- [ ] [AGENT] Azure Functions endpoints (auth, tournaments, matches, predict, leaderboard)
- [ ] [AGENT] Bot webhook endpoint для Telegram updates

### Фаза 2: Фронтенд — MVP (3-4 дня)

- [ ] [AGENT] Настроить Telegram Web App SDK
- [ ] [AGENT] Экран авторизации (автоматическая через initData)
- [ ] [AGENT] Экран регистрации (только игровой ник) + сохранение через API
- [ ] [AGENT] Экран турнира (текущий матч + навигация)
- [ ] [AGENT] Карточка матча — все 5 состояний (Upcoming / Open / Locked / Resolved / Canceled)
- [ ] [AGENT] Кнопки прогноза (Мирные/Мафия + сетка 1-10 + Никто)
- [ ] [AGENT] Отображение % голосов в Open/Locked/Resolved-состояниях
- [ ] [AGENT] Таблица лидеров (Leaderboard)
- [ ] [AGENT] Polling-клиент для `match-state-<id>.json`
- [ ] [AGENT] Адаптивные интервалы polling по состоянию матча
- [ ] [AGENT] Telegram MainButton для "Сохранить прогноз"
- [ ] [AGENT] Адаптация темы (светлая/тёмная по Telegram)

### Фаза 3: Админ-панель (1-2 дня)

- [ ] [AGENT] Отдельный URL или секция в Mini App (по Telegram ID)
- [ ] [AGENT] Список матчей с текущим состоянием
- [ ] [AGENT] Кнопки переключения состояний (Open → Lock → Resolve / Cancel)
- [ ] [AGENT] Форма ввода правильных ответов (Dropdown: сторона + ушедший игрок)
- [ ] [AGENT] Создание нового матча (номер игры, стол)

### Фаза 4: Регистрация и профиль (1 день)

- [ ] [AGENT] Эндпоинт `POST /api/me/nickname` с валидацией ника
- [ ] [AGENT] Проверка уникальности `GameNickname` и нормализация ввода
- [ ] [AGENT] Блокировка `POST /api/matches/{id}/predict` для незарегистрированных
- [ ] [AGENT] UI-индикатор текущего ника в профиле

### Фаза 5: Полировка и тестирование (2-3 дня)

- [ ] [AGENT] Нагрузочное тестирование (симуляция 100+ одновременных пользователей)
- [ ] [MANUAL] Тестирование на реальных устройствах (Android + iOS Telegram)
- [ ] [AGENT] Edge cases из PRD: удаление по фолу, попил, АФК
- [ ] [AGENT] FAQ / Rules страница в Mini App
- [ ] [MANUAL] Страница призов (фото мерча)
- [ ] [MANUAL] Мониторинг (логи Azure Functions, Blob read/write metrics)

---

## Инфраструктурные зависимости

| Что нужно | Откуда | Как использовать |
|-----------|--------|------------------|
| Azure SQL Database | Готовая | Отдельная изолированная схема `pickem` без связей с другими схемами |
| Telegram Bot | Новый/существующий | Запуск Mini App и служебные webhook-события |
| Azure подписка | Готовая | Деплой Functions + Blob Storage |
| Static hosting Mini App | Azure Static Web Apps | Хостинг Telegram Mini App (HTTPS URL для BotFather Web App) |

---

## Оценка затрат (Azure, ежемесячно)

| Сервис | Tier | Стоимость |
|--------|------|-----------|
| Azure Functions | Consumption (Free grants: 1M exec/мес) | ~$0 |
| Azure Blob Storage | Hot LRS (JSON state) | ~$0-2 |
| Azure Static Web Apps | Free | $0 |
| Azure SQL | Готовая БД | $0 |
| **Итого (MVP)** | | **$0** |
| **Итого (Production)** | | **~$0-10/мес** |

---

## Критический путь (MVP для первого турнира)

```
Фаза 0 → Фаза 1 → Фаза 2 → Фаза 3 → Фаза 4 → Тестирование
```

Минимальный MVP (обязательная регистрация по нику):
**Фаза 0 + Фаза 1 + Фаза 2 + Фаза 3 + Фаза 4 ≈ 8-12 дней**
