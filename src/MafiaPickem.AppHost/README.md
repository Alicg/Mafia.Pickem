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
- **Azurite Blob**: http://127.0.0.1:10000
- **Azurite Queue**: http://127.0.0.1:10001
- **Azurite Table**: http://127.0.0.1:10002
- **Backend API**: http://localhost:7071/api/
- **Frontend**: http://localhost:5173

## Requirements

- .NET 8 SDK
- Node.js and npm
- Azure Functions Core Tools v4 (`npx func start` must be available)

## User-Secrets

AppHost supports configuration from user-secrets. Example:

```bash
dotnet user-secrets set --project src/MafiaPickem.AppHost SqlConnectionString "Server=localhost;Database=MafiaPickem;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set --project src/MafiaPickem.AppHost TelegramBotToken "YOUR_BOT_TOKEN_HERE"
```

You can list configured values with:

```bash
dotnet user-secrets list --project src/MafiaPickem.AppHost
```

## Telemetry

The backend now emits OpenTelemetry logs, metrics, and traces.

- Local development uses Aspire only.
- `backend` gets its OTLP endpoint from `.WithOtlpExporter()`, so logs, metrics, and traces appear in the Aspire dashboard.
- AppHost does not forward any external Grafana OTLP settings in local development.

Production is separate from AppHost.

- `src/MafiaPickem.AppHost` is a local orchestrator and should not be deployed.
- In production, configure the Function App with the standard OpenTelemetry settings:
  - `OTEL_EXPORTER_OTLP_ENDPOINT`
  - `OTEL_EXPORTER_OTLP_HEADERS`
  - `OTEL_EXPORTER_OTLP_PROTOCOL` (optional)
- With those settings, both the Functions host and the .NET isolated worker send telemetry to Grafana.

Example production configuration:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=https://your-grafana-otlp-endpoint
OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic ...
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
```

## Notes

- All services are managed by Aspire and will be terminated when the AppHost stops
- Logs and telemetry for all services are available in the Aspire dashboard
- Azure Functions `host.json` uses `telemetryMode: OpenTelemetry`, and the .NET isolated worker is instrumented with OpenTelemetry as well
- The AppHost automatically handles service lifecycle and health monitoring
- If Azurite is not recognized globally, AppHost still uses `npx --yes azurite`
