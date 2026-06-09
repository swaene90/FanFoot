# Fanfoot

A Blazor Interactive Server app for tracking Sleeper fantasy football leagues, with an AI assistant powered by a local Ollama model.

## Features

- Sign in with your Sleeper username — leagues import automatically
- View league standings, team rosters, and matchup history
- Supports redraft, keeper, and dynasty leagues
- Dynasty-specific: full draft pick holdings with projected draft order
- AI chat assistant with full league context (rosters, standings, trade values)
- AI knows your current week's matchup and opponent roster
- Trade value data from FantasyCalc (dynasty and redraft)
- Real-time player stats and NFL news via tools
- Chat history — last 10 sessions per user persisted to the database
- Player data synced nightly via background service or on-demand

## Tech Stack

- **.NET 10** Blazor Interactive Server
- **PostgreSQL** via EF Core + Npgsql
- **Ollama** for local AI inference (`qwen2.5:7b` by default)
- **Sleeper API** for league, roster, and player data
- **FantasyCalc API** for trade values
- **ESPN API** for player news
- **Docker** for deployment

## Running Locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL instance
- [Ollama](https://ollama.com) running locally with `qwen2.5:7b` pulled

### Configuration

Copy the example env and fill in your values:

```bash
cp .env.example .env
```

| Variable | Description |
|---|---|
| `DB_CONNECTION_STRING` | PostgreSQL connection string |
| `OllamaUrl` | Ollama base URL (default: `http://localhost:11434/`) |

### Run

```bash
cd src/Fanfoot.Web
dotnet run
```

Open `http://localhost:5020` and sign in with your Sleeper username.

## Docker

```bash
docker compose up --build
```

The app runs on port `5020`. Configure `DB_CONNECTION_STRING` and `OllamaUrl` in your `.env` file before starting.

See [DEPLOY.md](DEPLOY.md) for setting up auto-deploy to an Unraid server via GitHub Actions.

## Project Structure

```
Fanfoot.slnx
src/
├── Fanfoot.Domain/          # Entity models
├── Fanfoot.Infrastructure/  # EF Core DbContext, Sleeper/FantasyCalc clients, mappings
└── Fanfoot.Web/
    ├── Components/Pages/     # Blazor pages (Home, Chat, LeagueDetail, TeamDetail)
    ├── Services/             # ChatService, PlayerImportService
    └── wwwroot/              # Static assets
```

## License

MIT
