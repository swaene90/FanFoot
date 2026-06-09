# Local Development

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Setup

**1. Start the local database**

```powershell
docker compose --profile local up -d
```

This starts a PostgreSQL 17 container on `localhost:5432` with a persistent volume (`fanfoot-dev-data`).

**2. Run the app**

```powershell
cd src/Fanfoot.Web
dotnet run
```

App runs at http://localhost:5020.

## First Run

On first startup the app will automatically:

1. Apply all pending EF Core migrations
2. Seed the database with:
   - A local user (`admin@local.dev` / `password`)
   - A dev league
   - All NFL player data from `src/Fanfoot.Web/Data/players.json`

Seeding is idempotent — subsequent restarts skip it if data already exists.

## Player Data

`src/Fanfoot.Web/Data/players.json` is a snapshot of the Sleeper NFL players API. To refresh it:

```powershell
Invoke-WebRequest https://api.sleeper.app/v1/players/nfl -OutFile src/Fanfoot.Web/Data/players.json
```

## EF Core Migrations

Run from `src/Fanfoot.Web/`:

```powershell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Targets the local PostgreSQL container (`localhost:5432`).
