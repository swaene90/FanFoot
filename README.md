# Fanfoot

A React single-page app with an ASP.NET Core API for tracking Sleeper fantasy football leagues, with an AI assistant powered by a local Ollama model.

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
- Player data synced nightly via a background service

## Tech Stack

- **React + TypeScript + Vite** single-page client
- **.NET 10** API host with cookie authentication
- **PostgreSQL** via EF Core + Npgsql
- **DeepSeek**, **Groq**, or **Ollama** for AI inference, selectable from the AI chat screen
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

The app runs on port `5020`. Configure `DB_CONNECTION_STRING`, `OLLAMA_URL`, and optionally `DEEPSEEK_API_KEY` or `GROQ_API_KEY` in `.env`. Select a configured provider and model from the AI chat screen.

See [docs/DEPLOY.md](docs/DEPLOY.md) for setting up auto-deploy to an Unraid server via GitHub Actions.

## Project Structure

The app is organized into three layers — **Controllers** (API endpoints + client-facing DTOs), **Domain** (application logic + domain models), and **Infrastructure** (EF Core + external API clients):

```
src/Fanfoot.Web/
├── Controllers/      # API controllers + request/response DTOs
├── ClientApp/        # React/Vite application, built into wwwroot
├── Domain/           # Domain models + application services
└── Infrastructure/   # DbContext, entities, Sleeper/FantasyCalc/ESPN/LLM clients
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full layout and layering rules.

## License

MIT
