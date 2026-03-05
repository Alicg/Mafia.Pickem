# MafiaPickem.AppHost

This is the .NET Aspire orchestration host for the Mafia Pickem application.

## Overview

The AppHost orchestrates three managed processes:

1. **Azurite** - Azure Storage emulator
2. **Backend** - Azure Functions API
3. **Frontend** - Vite dev server

## Running the Application

To start all services:

```bash
cd src/MafiaPickem.AppHost
dotnet run
```

Or from the solution root:

```bash
dotnet run --project src/MafiaPickem.AppHost
```

The Aspire dashboard will be available at `http://localhost:15000` by default.

## Service Dependencies

The services start in this order:
1. Azurite starts first
2. Backend starts after Azurite is ready
3. Frontend starts after Backend is ready

## Endpoints

- **Aspire Dashboard**: http://localhost:15000
- **Azurite Blob**: http://localhost:10000
- **Azurite Queue**: http://localhost:10001
- **Azurite Table**: http://localhost:10002
- **Backend API**: http://localhost:7071
- **Frontend**: http://localhost:5173

## Requirements

- .NET 8 SDK
- Node.js and npm
- Azure Functions Core Tools v4 (`npx func start` must be available)

## Notes

- All services are managed by Aspire and will be terminated when the AppHost stops
- Logs and telemetry for all services are available in the Aspire dashboard
- The AppHost automatically handles service lifecycle and health monitoring
