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

See [docs/local-development.md](docs/local-development.md) for full setup instructions.

## Docker

```bash
docker compose up --build
```

The app runs on port `5020`. Configure `DB_CONNECTION_STRING` and `OllamaUrl` in your `.env` file before starting.

See [docs/DEPLOY.md](docs/DEPLOY.md) for setting up auto-deploy to an Unraid server via GitHub Actions.

## Project Structure

The app is organized into three layers — **Controllers** (API endpoints + client-facing DTOs), **Domain** (application logic + domain models), and **Infrastructure** (EF Core + external API clients):

```
src/Fanfoot.Web/
├── Controllers/      # API controllers + request/response DTOs
├── Components/       # Blazor pages and layout
├── Domain/           # Domain models + application services
└── Infrastructure/   # DbContext, entities, Sleeper/FantasyCalc/ESPN/LLM clients
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full layout and layering rules.

## License

MIT
