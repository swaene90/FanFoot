# Architecture

Fanfoot is a single ASP.NET Core project (`src/Fanfoot.Web`) organized into three distinct layers, separated by folder and namespace. Dependencies flow downward only:

```
React client / Controllers (API)
        │
        ▼
      Domain  (application logic + domain models)
        │
        ▼
  Infrastructure  (database + external APIs)
```

## Layout

```
src/Fanfoot.Web/
├── Controllers/                 # API endpoints + client-facing DTOs
│   ├── AuthController.cs        #   session, login, registration, CSRF endpoints
│   ├── MeController.cs          #   profile and preferences DTO endpoints
│   ├── LeaguesController.cs     #   member-scoped league, roster, and draft endpoints
│   └── ChatController.cs        #   owner-scoped chat endpoints
│
├── ClientApp/                   # React/Vite SPA, built into wwwroot
│   └── src/                     #   routes, API client, and custom CSS
│
├── Domain/                      # namespace Fanfoot.Domain.*
│   ├── Models/                  #   domain models (League, Team, Player, User,
│   │                            #   DraftInfo, TradedPick, TeamRoster, ...)
│   └── Services/                #   all application logic
│       ├── LeagueService.cs     #   league/team/roster queries + Sleeper imports
│       ├── UserService.cs       #   user profile + teams-by-season queries
│       ├── AuthService.cs       #   sign-in, registration, league authorization
│       ├── ChatService.cs       #   AI assistant context building + tool loop
│       ├── PreferencesService.cs#   dark-mode preference
│       └── PlayerImportService.cs  # nightly background import
│
└── Infrastructure/              # namespace Fanfoot.Infrastructure.*
    ├── Data/
    │   ├── Entities/            #   EF Core persistence entities (*Entity)
    │   ├── FanfootDbContext.cs  #   explicit ToTable() mappings
    │   └── DatabaseSeeder.cs    #   dev-only seed (local user, league, players)
    ├── Clients/                 #   external HTTP clients + their DTOs
    │   ├── SleeperClient.cs     #   Sleeper API (leagues, rosters, players, drafts)
    │   ├── FantasyCalcClient.cs #   trade values
    │   ├── EspnClient.cs        #   player news
    │   └── LlmClient.cs         #   Groq/Ollama chat completions (OpenAI-compatible)
    └── Mapping/
        ├── SleeperMapper.cs     #   Sleeper DTO → entity / domain model
        └── EntityMapper.cs      #   entity ↔ domain model
```

## Rules

**Models exist in three flavors, one per layer.**

| Kind | Location | Example |
|------|----------|---------|
| Client-facing DTOs | `Api/Dtos/` | `LeagueDto`, `RosterDto` |
| Domain models | `Domain/Models/` | `League`, `TeamRoster` |
| Persistence entities & external API DTOs | `Infrastructure/` | `LeagueEntity`, `SleeperLeagueDto` |

**Domain services own all application logic.** They inject `FanfootDbContext` directly (no repository layer) and the Infrastructure clients, but always return domain models — entities and external DTOs never cross out of a service's public API. `EntityMapper` converts at the boundary.

**The React client only accesses API DTOs.** Controllers and Domain services retain Infrastructure access; external API clients never run in the browser.

**Entities map to the original table names.** The `*Entity` split was done without a schema change — `FanfootDbContext` pins table names with `ToTable()`, and `dotnet ef migrations has-pending-model-changes` should stay clean after any rename-only refactor.

## Registration

- `AddFanfootInfrastructure(connectionString)` (`Infrastructure/ServiceExtensions.cs`) — DbContext, typed HTTP clients, `LlmClient`
- `AddFanfootDomain()` (`Domain/ServiceExtensions.cs`) — all Domain services + the `PlayerImportService` hosted service

Both are called from `Program.cs`, which otherwise contains host wiring (cookie auth, antiforgery, SPA static files, the named "Ollama" HttpClient, migration/seed on startup).
