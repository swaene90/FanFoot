# Fantfoot

A Blazor Interactive Server app for tracking fantasy football leagues via the [Sleeper](https://sleeper.com) API.

## Features

- Import Sleeper leagues, teams/rosters, and users into a local SQLite database
- View league standings with wins, losses, ties, points for/against
- Team names pulled from Sleeper user metadata alongside manager display names
- Supports any Sleeper league by ID

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

## Project Structure

```
Fantfoot.slnx
src/
├── Fantfoot.Domain/       # Entity models (League, Team, User, etc.)
├── Fantfoot.Infrastructure/  # Sleeper API client, EF Core DbContext, services, mappings
└── Fantfoot.Web/          # Blazor UI (Home, LeagueDetail pages)
```

## License

MIT
