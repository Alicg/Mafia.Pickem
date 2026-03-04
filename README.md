# Mafia Pick'em

Telegram Mini App для прогнозов на матчи мафии. Backend на Azure Functions (.NET 8), frontend на React + Vite + TypeScript.

## Запуск локально

### Требования

- [Node.js](https://nodejs.org/) (v18+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) v4
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (эмулятор Azure Storage)
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

### 1. Запуск Azurite (эмулятор Azure Storage)

```bash
azurite --silent --skipApiVersionCheck
```

Azurite стартует на портах по умолчанию:
- Blob: `127.0.0.1:10000`
- Queue: `127.0.0.1:10001`
- Table: `127.0.0.1:10002`

### 2. Запуск Azure Functions (backend)

```bash
cd src/MafiaPickem.Api
func start
```

API будет доступен на `http://localhost:7071/api/`.

> При первом запуске убедитесь, что `local.settings.json` содержит корректные значения для `SqlConnectionString` и `TelegramBotToken`.

### 3. Запуск Frontend

```bash
cd src/frontend
npm install
npm run dev
```

Frontend запустится на `http://localhost:5173/`. API-запросы к `/api/*` автоматически проксируются на backend (`localhost:7071`), blob-запросы к `/blob/*` — на Azurite (`127.0.0.1:10000`).

## Быстрый старт (все 3 терминала)

```
# Терминал 1 — Azurite
npx azurite --silent --skipApiVersionCheck

# Терминал 2 — Backend
cd src/MafiaPickem.Api && npx func start

# Терминал 3 — Frontend
cd src/frontend ; npm install ; npm run dev
```
