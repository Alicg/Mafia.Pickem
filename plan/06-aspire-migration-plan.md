# План миграции на .NET Aspire (Mafia Pick'em)

## 1. Цель миграции

Перевести локальную разработку на единый оркестратор `.NET Aspire AppHost`, сохранив текущую архитектуру продукта:
- Backend: Azure Functions (.NET 8 Isolated Worker)
- Frontend: React + Vite
- State: Azure Blob Storage (Azurite локально)
- DB: SQL Server

Ключевая цель: убрать ручной запуск нескольких процессов и стабилизировать локальную среду, не меняя бизнес-логику MVP.

## 2. Что остается без изменений

- API-контракты и маршруты `/api/*`.
- Механика polling blob-файлов `match-state-{id}.json` и `leaderboard-{id}.json`.
- Модель авторизации Telegram -> JWT.
- Структура доменных сервисов/repositories в `MafiaPickem.Api`.

## 3. Целевая dev-архитектура после миграции

1. `MafiaPickem.AppHost` поднимает и оркестрирует ресурсы/процессы.
2. `MafiaPickem.ServiceDefaults` подключает общие настройки observability/health/telemetry.
3. `Azurite` запускается как managed resource (container или executable).
4. Backend Functions запускается как managed executable (`func start`).
5. Frontend Vite запускается как managed executable (`npm run dev`).
6. Конфигурация и секреты передаются централизованно из AppHost в процессы.

## 4. Требования по CORS и проксированию (обязательные)

Текущая стратегия обхода CORS должна сохраниться:
- Frontend обращается только к origin Vite dev server.
- Vite проксирует:
  - `/api/*` -> Azure Functions (`http://localhost:7071`)
  - `/blob/*` -> Azurite Blob endpoint (`http://127.0.0.1:10000/devstoreaccount1/match-states/*`)

### 4.1. Целевое состояние прокси

1. Оставить прокси в `vite.config.ts` как основной механизм CORS-обхода.
2. Убрать жесткий хардкод портов в пользу env-переменных:
   - `VITE_API_BASE=http://localhost:7071`
   - `VITE_BLOB_BASE=http://127.0.0.1:10000`
   - `VITE_BLOB_ACCOUNT=devstoreaccount1`
   - `VITE_BLOB_CONTAINER=match-states`
3. Формировать `rewrite` для `/blob` через env, чтобы сохранить текущий путь в Azurite.
4. Проверить, что браузер не ходит напрямую на `7071`/`10000` (только через Vite origin).

### 4.2. Проверки CORS после миграции

- Нет preflight-ошибок при вызовах `/api/*`.
- Blob polling работает через `/blob/*` без CORS-ошибок.
- В network trace frontend origin один и тот же (порт Vite).

## 5. Поэтапный план миграции

## Фаза 0. Подготовка (0.5 дня)

1. Зафиксировать baseline:
   - Команда запуска сегодня: `azurite`, `func start`, `npm run dev`.
   - Smoke checklist (auth, список турниров, polling match-state, leaderboard).
2. Создать отдельную ветку миграции (`feature/aspire-apphost`).
3. Подготовить rollback:
   - Старый workflow из `README.md` сохраняется до завершения Фазы 4.

Критерий выхода:
- Есть reproducible baseline и зафиксированный smoke-сценарий.

## Фаза 1. Bootstrap Aspire (1 день)

1. Добавить проекты:
   - `src/MafiaPickem.AppHost`
   - `src/MafiaPickem.ServiceDefaults`
2. Добавить их в `MafiaPickem.sln`.
3. Настроить AppHost:
   - ресурс Azurite;
   - executable для Functions (`workingDir=src/MafiaPickem.Api`, command `npx func start`);
   - executable для Frontend (`workingDir=src/frontend`, command `npm run dev`).
4. Добавить зависимости запуска (frontend зависит от backend и azurite readiness).

Критерий выхода:
- `dotnet run --project src/MafiaPickem.AppHost` поднимает все dev-сервисы.

## Фаза 2. Централизация конфигурации (1 день)

1. Вынести ключевые переменные в AppHost env:
   - `SqlConnectionString`
   - `BlobStorageConnectionString`
   - `BlobContainerName`
   - `JwtSecret`, `JwtIssuer`
   - `PickemAdminTelegramIds`
   - `TelegramBotToken`, `TelegramWebhookSecretToken`
2. Для фронта передавать env прокси (`VITE_API_BASE`, `VITE_BLOB_BASE`, ...).
3. Сохранить `local.settings.json` как fallback для запуска вне Aspire.
4. Документировать, какие переменные обязательны локально.

Критерий выхода:
- Backend и Frontend стартуют через AppHost без ручного редактирования локальных файлов при каждом запуске.

## Фаза 3. Прокси и CORS hardening (0.5-1 день)

1. Адаптировать `vite.config.ts` под env-параметры target/rewrite.
2. Добавить дефолты для текущих локальных портов (`7071`, `10000`) при отсутствии env.
3. Проверить сценарии:
   - авторизация и вызовы `/api/*`;
   - polling `match-state-*` через `/blob/*`;
   - загрузка leaderboard blob.
4. Зафиксировать правила: frontend не вызывает backend/blob напрямую.

Критерий выхода:
- CORS-ошибки отсутствуют во всех ключевых пользовательских потоках.

## Фаза 4. Observability и DX (0.5 дня)

1. Подключить `ServiceDefaults` (telemetry/health минимум).
2. Проверить логирование в Aspire Dashboard:
   - Functions startup logs;
   - frontend dev logs;
   - azurite logs.
3. Описать стандартный dev-старт одной командой.

Критерий выхода:
- Диагностика и запуск проекта доступны из единой точки входа.

## Фаза 5. Документация и завершение (0.5 дня)

1. Обновить `README.md`:
   - новый основной сценарий запуска через Aspire;
   - legacy-сценарий оставить как fallback.
2. Обновить `plan/PRD.md` на уровне high-level (без реализации).
3. Добавить troubleshooting-блок:
   - если локально не найден `azurite`: использовать `npx azurite`;
   - если не стартует `func`: проверка Core Tools и PATH.

Критерий выхода:
- Команда может поднять проект с нуля по обновленной документации.

## 6. Риски и меры

1. Риск: нестабильный запуск `func` как executable в AppHost.
   - Мера: fallback task/script + отдельная проверка `npx func --version`.
2. Риск: разъезд портов ломает proxy rewrite.
   - Мера: все target-порты через env, дефолты фиксированы в одном месте.
3. Риск: различия Windows/macOS/Linux в командах.
   - Мера: использовать кроссплатформенные команды и `npx`.
4. Риск: утечка секретов в репозиторий.
   - Мера: секреты только из user-secrets/env, без коммита реальных значений.

## 7. Критерии готовности миграции (Definition of Done)

1. AppHost поднимает минимум 3 компонента: Azurite, Functions, Frontend.
2. Весь трафик браузера идет через Vite proxy (`/api`, `/blob`), CORS-ошибок нет.
3. Smoke flow проходит:
   - dev auth;
   - создание/обновление прогноза;
   - обновление crowd stats из blob;
   - leaderboard.
4. README и плановые документы обновлены.
5. Legacy запуск остается рабочим как аварийный сценарий.

## 8. Предлагаемая последовательность внедрения в git

1. Commit 1: Bootstrap AppHost + ServiceDefaults.
2. Commit 2: Env wiring + frontend proxy parametrization.
3. Commit 3: Smoke fixes + CORS hardening.
4. Commit 4: Docs update (README + PRD + план миграции).

## 9. Оценка по времени

- Общая оценка: 3-4 дня.
- Минимально жизнеспособный переход (без расширенной observability): 1.5-2 дня.
