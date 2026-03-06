# Mafia Pick'em

Telegram Mini App для прогнозов на матчи мафии. Backend на Azure Functions (.NET 8), frontend на React + Vite + TypeScript.

## Запуск локально

### Требования

- [Node.js](https://nodejs.org/) (v18+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) v4
- SQL Server (локальный или Docker)

### Подготовка базы данных

Создайте БД `MafiaPickem` и выполните миграцию:

```sql
-- В SQL Server Management Studio или sqlcmd
CREATE DATABASE MafiaPickem;
GO
```

Затем примените скрипт схемы:

```bash
sqlcmd -S localhost -d MafiaPickem -i sql/001-create-schema.sql
```

### 1. Основной сценарий: запуск через .NET Aspire AppHost

```bash
dotnet run --project src/MafiaPickem.AppHost
```

После старта:
- Aspire Dashboard: `http://localhost:15000`
- Frontend (Vite): `http://localhost:5173`
- Backend API: `http://localhost:7071/api/`
- Azurite Blob: `http://127.0.0.1:10000`

AppHost поднимает и оркестрирует:
- Azurite (`npx --yes azurite --silent --skipApiVersionCheck`)
- Backend (`npx func start --port 7071`)
- Frontend (`npm run dev -- --port 5173 --strictPort`)

### 2. User-secrets для AppHost (рекомендуется)

AppHost читает конфигурацию из env и user-secrets. Для локалки задайте секреты:

```bash
dotnet user-secrets set --project src/MafiaPickem.AppHost SqlConnectionString "Server=localhost;Database=MafiaPickem;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set --project src/MafiaPickem.AppHost BlobStorageConnectionString "UseDevelopmentStorage=true"
dotnet user-secrets set --project src/MafiaPickem.AppHost BlobContainerName "match-states"
dotnet user-secrets set --project src/MafiaPickem.AppHost JwtSecret "your-local-dev-jwt-secret-at-least-32-characters-long"
dotnet user-secrets set --project src/MafiaPickem.AppHost JwtIssuer "MafiaPickem"
dotnet user-secrets set --project src/MafiaPickem.AppHost PickemAdminTelegramIds "999999999"
dotnet user-secrets set --project src/MafiaPickem.AppHost TelegramBotToken "YOUR_BOT_TOKEN_HERE"
dotnet user-secrets set --project src/MafiaPickem.AppHost TelegramWebhookSecretToken "your-webhook-secret-token-here"
dotnet user-secrets set --project src/MafiaPickem.AppHost TelegramWebhookUrl "https://pickem.markery.online/api/bot/webhook"
dotnet user-secrets set --project src/MafiaPickem.AppHost TelegramMiniAppUrl "https://pickem.markery.online"
```

Прокси-переменные для frontend (опционально, у них есть дефолты):

```bash
dotnet user-secrets set --project src/MafiaPickem.AppHost VITE_DEV_PROXY_API_TARGET "http://localhost:7071"
dotnet user-secrets set --project src/MafiaPickem.AppHost VITE_DEV_PROXY_BLOB_TARGET "http://127.0.0.1:10000"
dotnet user-secrets set --project src/MafiaPickem.AppHost VITE_DEV_PROXY_BLOB_ACCOUNT "devstoreaccount1"
dotnet user-secrets set --project src/MafiaPickem.AppHost VITE_DEV_PROXY_BLOB_CONTAINER "match-states"
```

### 3. Legacy fallback: запуск в 3 терминалах

Если AppHost недоступен, используйте старый сценарий:

```bash
# Терминал 1 — Azurite
npx azurite --silent --skipApiVersionCheck

# Терминал 2 — Backend
cd src/MafiaPickem.Api && npx func start --port 7071

# Терминал 3 — Frontend
cd src/frontend ; npm install ; npm run dev
```

### 4. CORS и proxy поведение

- Браузер должен ходить только на origin Vite (`http://localhost:5173`).
- Vite проксирует `/api/*` на backend.
- Vite проксирует `/blob/*` на Azurite Blob endpoint с rewrite в `/{account}/{container}`.

Env-модель фронтенда разделена на 2 уровня:
- Dev proxy target (читает Vite server): `VITE_DEV_PROXY_API_TARGET`, `VITE_DEV_PROXY_BLOB_TARGET`, `VITE_DEV_PROXY_BLOB_ACCOUNT`, `VITE_DEV_PROXY_BLOB_CONTAINER`.
- Browser fetch base (читает runtime-клиент): `VITE_BROWSER_API_BASE_URL`, `VITE_BROWSER_BLOB_BASE_URL`.

Для локалки runtime-клиент должен оставаться на `/api` и `/blob`, чтобы не обходить Vite proxy.

Demo mode:
- `src/frontend/.env.demo` используется при запуске `npm run dev -- --mode demo`.

### 5. Troubleshooting

- `azurite` not recognized:
	- используйте `npx azurite --silent --skipApiVersionCheck`
	- проверьте `npm list -g --depth=0` и `%APPDATA%\npm` в `PATH`
- `func` not recognized или backend не стартует:
	- проверьте `npx func --version`
	- убедитесь, что установлен Azure Functions Core Tools v4
	- проверьте PATH/переустановите Core Tools
