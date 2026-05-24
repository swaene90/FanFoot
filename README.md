# Fantfoot

A Blazor Interactive Server app for tracking fantasy football leagues via the [Sleeper](https://sleeper.com) API.

## Features

- Import Sleeper leagues, teams/rosters, users, and players into a local SQLite database
- View league standings with wins, losses, ties, points for/against
- Team names pulled from Sleeper user metadata alongside manager display names
- View team rosters (Starters, Bench, Reserve/IR, Taxi Squad)
- Navigate between past seasons (auto-import previous season leagues)
- Supports any Sleeper league by ID
- Player data imported automatically via background service (daily at midnight Eastern) or on-demand via API

## Tech Stack

- **.NET 10** Blazor Interactive Server (targets `net10.0`)
- **EF Core 8** with SQLite
- **Sleeper API** (`https://api.sleeper.app/v1`)

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the app

```bash
cd src/Fantfoot.Web
dotnet run
```

Open `http://localhost:5020` in your browser.

### Import a league

1. Click **Import League**
2. Enter a Sleeper league ID (find yours from your league URL: `https://sleeper.com/leagues/<league_id>`)
3. The league, teams, and users will be imported and stored locally
4. From a league's detail page, click **Import Previous Season** to fetch past years

### Import players

Player data (all ~12,000+ NFL players) is imported automatically every night at midnight Eastern.
You can also trigger an import on demand:

```bash
curl -X POST http://localhost:5020/api/players/import
```

## Project Structure

```
Fantfoot.slnx
src/
├── Fantfoot.Domain/              # Entity models (League, Team, User, Player)
├── Fantfoot.Infrastructure/      # Sleeper API client, EF Core DbContext, services, mappings
│   ├── Clients/                  # HTTP client + DTOs for Sleeper API
│   ├── Data/                     # EF Core DbContext + migrations
│   ├── Mapping/                  # DTO → entity mappers
│   └── Services/                 # Business logic (LeagueService)
└── Fantfoot.Web/                 # Blazor UI pages + background service
    ├── Components/Pages/         # Razor pages (Home, LeagueDetail, TeamDetail)
    └── Services/                 # PlayerImportService (daily scheduler)
```

## License

MIT
