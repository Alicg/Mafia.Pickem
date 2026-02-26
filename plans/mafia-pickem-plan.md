## Plan: Mafia Pick'em Full Implementation

Implement the complete Mafia Pick'em Telegram Mini App — Azure Functions .NET 8 backend with Dapper/SQL, React+Vite+TypeScript frontend with Telegram Web App SDK, and Blob-based polling for real-time state updates. Uses Aspire + Azurite for local development, standalone SQL migrations, custom CSS, JWT auth tokens, and xUnit tests.

**Phases (9 phases)**

1. **Phase 1: Backend Project Setup & Domain Models**
    - **Objective:** Scaffold Azure Functions .NET 8 Isolated Worker project with Aspire, configure DI, define all domain models/DTOs/enums, SQL migration script.
    - **Files/Functions to Create:** Solution, Api project, Test project, Models/, Enums/, DTOs, Program.cs, host.json, local.settings.json, sql/001-create-schema.sql
    - **Tests:** MatchState enum tests, DTO validation tests
    - **Steps:** Create solution → add projects → NuGet packages → enums → entities → DTOs → DI setup → SQL script → tests

2. **Phase 2: Auth & User Management**
    - **Objective:** Telegram initData HMAC-SHA256 validation, JWT token generation, auth middleware, user repo, nickname service, auth/profile endpoints.
    - **Files/Functions:** TelegramAuthService, TelegramAuthMiddleware, UserContext, PickemUserRepository, NicknameService, AuthFunction, ProfileFunction
    - **Tests:** HMAC validation tests, nickname validation tests, user repo tests
    - **Steps:** Tests first → TelegramAuthService → NicknameService → UserRepo → middleware → endpoints → run tests

3. **Phase 3: Tournaments & Match State Machine**
    - **Objective:** Tournament/match repositories and full match state machine.
    - **Files/Functions:** TournamentRepository, MatchRepository, MatchStateService, TournamentFunctions, MatchFunctions
    - **Tests:** State machine transition tests, repository tests
    - **Steps:** Tests → MatchStateService → repositories → endpoints → run tests

4. **Phase 4: Predictions, Scoring & Leaderboard**
    - **Objective:** Prediction upsert, scoring formula, leaderboard aggregation.
    - **Files/Functions:** PredictionRepository, PredictionService, ScoringService, LeaderboardRepository, predict endpoint, leaderboard endpoint
    - **Tests:** Scoring formula tests, prediction validation tests
    - **Steps:** Tests → ScoringService → PredictionRepo/Service → LeaderboardRepo → endpoints → run tests

5. **Phase 5: Blob State Publishing & Admin Endpoints**
    - **Objective:** match-state JSON publishing to Blob with throttling, all admin endpoints.
    - **Files/Functions:** MatchStateDto, MatchStateBlobWriter, StatePublishService, AdminFunctions
    - **Tests:** Throttling policy tests, admin auth tests
    - **Steps:** Tests → BlobWriter → StatePublishService → AdminFunctions → wire side effects → run tests

6. **Phase 6: Bot Webhook**
    - **Objective:** Telegram bot webhook with secret validation.
    - **Files/Functions:** TelegramWebhookValidator, BotWebhookFunction
    - **Tests:** Webhook validator tests
    - **Steps:** Tests → validator → endpoint → run tests

7. **Phase 7: Frontend Setup, Auth & Registration**
    - **Objective:** Scaffold React+Vite+TS, Telegram SDK integration, auth flow, registration page.
    - **Files/Functions:** Vite project, App.tsx, telegram.ts, api.ts, useAuth hook, RegisterPage, theme
    - **Tests:** API client tests, auth hook tests
    - **Steps:** Scaffold → SDK → API client → auth flow → registration → theme → tests

8. **Phase 8: Frontend Match UI, Polling & Leaderboard**
    - **Objective:** Tournament screen, match card (5 states), predictions, crowd stats, polling, leaderboard.
    - **Files/Functions:** TournamentPage, MatchCard, PredictionForm, CrowdStats, PlayerGrid, LeaderboardPage, useMatchState
    - **Tests:** Component tests, polling hook tests
    - **Steps:** Polling hook → MatchCard → PredictionForm → CrowdStats → pages → MainButton → tests

9. **Phase 9: Frontend Admin Panel**
    - **Objective:** Admin panel with match management and result entry.
    - **Files/Functions:** AdminPage, MatchList, MatchStateControls, ResolveForm, CreateMatchForm
    - **Tests:** Admin page tests
    - **Steps:** Route guard → components → compose page → tests

**Resolved Decisions:**
1. Azurite + Aspire for local dev
2. Standalone .sql file for migrations
3. Custom CSS for minimal bundle size
4. JWT tokens for session auth
5. xUnit for testing
