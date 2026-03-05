## Phase 1 Complete: Bootstrap Aspire Host

Added Aspire bootstrap infrastructure to orchestrate local Azurite, Azure Functions, and Vite frontend from a single AppHost entrypoint. The solution now builds successfully with the new projects, and AppHost starts and serves the Aspire dashboard.

**Files created/changed:**
- `.gitignore`
- `MafiaPickem.sln`
- `src/MafiaPickem.AppHost/MafiaPickem.AppHost.csproj`
- `src/MafiaPickem.AppHost/Program.cs`
- `src/MafiaPickem.AppHost/Properties/launchSettings.json`
- `src/MafiaPickem.AppHost/README.md`
- `src/MafiaPickem.AppHost/appsettings.json`
- `src/MafiaPickem.AppHost/appsettings.Development.json`
- `src/MafiaPickem.ServiceDefaults/MafiaPickem.ServiceDefaults.csproj`
- `src/MafiaPickem.ServiceDefaults/Extensions.cs`

**Functions created/changed:**
- Top-level AppHost orchestration pipeline in `src/MafiaPickem.AppHost/Program.cs`.
- `AddServiceDefaults` extension placeholder in `src/MafiaPickem.ServiceDefaults/Extensions.cs`.

**Tests created/changed:**
- Build validation: `dotnet build MafiaPickem.sln` (passing).
- Runtime smoke validation: `dotnet run --project src/MafiaPickem.AppHost` (AppHost started and dashboard exposed).

**Review Status:** APPROVED with minor recommendations

**Git Commit Message:**
feat: bootstrap aspire apphost orchestration

- add AppHost and ServiceDefaults projects to solution
- orchestrate azurite, functions, and vite startup order
- harden local startup with fixed ports and dashboard launch profile
- ignore src build artifacts via wildcard bin/obj rules