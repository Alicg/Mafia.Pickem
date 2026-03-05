## Plan: Aspire Migration Integration

Migrate local development orchestration to .NET Aspire AppHost without changing product business logic. The rollout is split into incremental phases: bootstrap orchestration, centralize configuration (including user-secrets), harden Vite proxy/CORS behavior, then finalize observability and documentation.

**Phases 5**
1. **Phase 1: Bootstrap Aspire Host**
    - **Objective:** Add AppHost and ServiceDefaults projects and orchestrate Azurite, Functions, and Frontend with startup dependencies.
    - **Files/Functions to Modify/Create:** `src/MafiaPickem.AppHost/*`, `src/MafiaPickem.ServiceDefaults/*`, `MafiaPickem.sln`, `.gitignore`.
    - **Tests to Write:** Build verification for solution and AppHost startup smoke check.
    - **Steps:**
        1. Create `MafiaPickem.AppHost` and `MafiaPickem.ServiceDefaults` projects.
        2. Add projects to `MafiaPickem.sln`.
        3. Configure AppHost executables for `npx azurite`, `npx func start`, and `npm run dev` with dependency chain.
        4. Run `dotnet build MafiaPickem.sln` and `dotnet run --project src/MafiaPickem.AppHost`.

2. **Phase 2: Centralize Env and User-Secrets**
    - **Objective:** Move key backend/frontend configuration into AppHost env wiring with secure local user-secrets support.
    - **Files/Functions to Modify/Create:** `src/MafiaPickem.AppHost/Program.cs`, `src/MafiaPickem.AppHost/MafiaPickem.AppHost.csproj`, `src/MafiaPickem.Api/local.settings.json` (fallback docs), optionally README sections.
    - **Tests to Write:** Startup checks that backend and frontend receive expected env values from AppHost.
    - **Steps:**
        1. Add configuration binding in AppHost for required values and defaults.
        2. Wire sensitive values via `dotnet user-secrets` in AppHost.
        3. Pass backend variables (`SqlConnectionString`, `BlobStorageConnectionString`, `BlobContainerName`, `JwtSecret`, `JwtIssuer`, `PickemAdminTelegramIds`, `TelegramBotToken`, `TelegramWebhookSecretToken`).
        4. Pass frontend proxy variables (`VITE_API_BASE`, `VITE_BLOB_BASE`, `VITE_BLOB_ACCOUNT`, `VITE_BLOB_CONTAINER`).

3. **Phase 3: Vite Proxy and CORS Hardening**
    - **Objective:** Parameterize Vite proxy target/rewrite via env, preserving single-origin browser traffic through Vite.
    - **Files/Functions to Modify/Create:** `src/frontend/vite.config.ts`, optional frontend env sample docs.
    - **Tests to Write:** Proxy behavior checks for `/api/*` and `/blob/*` with no CORS/preflight errors.
    - **Steps:**
        1. Replace hardcoded proxy target and rewrite with env-driven values and safe defaults.
        2. Confirm blob rewrite path keeps `devstoreaccount1/match-states` behavior.
        3. Verify frontend calls only Vite origin.
        4. Validate key flows: auth, match-state polling, leaderboard polling.

4. **Phase 4: ServiceDefaults and Observability**
    - **Objective:** Add minimum shared defaults for telemetry/health and ensure dashboard visibility of logs.
    - **Files/Functions to Modify/Create:** `src/MafiaPickem.ServiceDefaults/Extensions.cs`, AppHost wiring as needed.
    - **Tests to Write:** Startup validation that services are visible and logs are accessible via Aspire dashboard.
    - **Steps:**
        1. Implement minimal service default extension points.
        2. Ensure AppHost captures logs for Azurite, backend, and frontend.
        3. Keep scope minimal and non-invasive to existing business logic.

5. **Phase 5: Documentation and Completion**
    - **Objective:** Make Aspire the default local startup path and keep legacy fallback documented.
    - **Files/Functions to Modify/Create:** `README.md`, `plan/PRD.md`.
    - **Tests to Write:** Documentation-driven smoke test from clean local startup instructions.
    - **Steps:**
        1. Update README with one-command Aspire flow and legacy fallback flow.
        2. Add troubleshooting notes for Azurite and Functions Core Tools.
        3. Update high-level PRD notes for local orchestration.
        4. Re-run smoke checklist and finalize migration report.

**Open Questions 3**
1. Should we store all non-secret defaults in AppHost `appsettings.*` or keep them hardcoded in `Program.cs` with fallback logic?
2. Do you want a committed `.env.example` in `src/frontend` for proxy variables, or only AppHost-managed env during runtime?
3. Is `npx func start` mandatory, or can we switch to deterministic package-pinned invocation if local machines have inconsistencies?