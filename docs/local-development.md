# Local Development

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Setup

Start the complete local stack:

```powershell
docker compose --profile local up --build -d --wait
```

This starts the PostgreSQL 17 database, builds and runs the API, and builds and serves the React UI. The app is available at http://localhost:5020 and PostgreSQL is available at `localhost:5432`. Data is persisted in the `fanfoot-db-data` Docker volume.

Stop the stack with:

```powershell
docker compose --profile local down
```

Re-run the startup command after source changes to rebuild and restart the API and UI containers.

## First Run

On first startup the app will automatically:

1. Apply all pending EF Core migrations

## Player Data

`src/Fanfoot.Web/localPlayerData.json` is a snapshot of the Sleeper NFL players API. To refresh it:

```powershell
Invoke-WebRequest https://api.sleeper.app/v1/players/nfl -OutFile src/Fanfoot.Web/localPlayerData.json
```

## EF Core Migrations

Run from `src/Fanfoot.Web/`:

```powershell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Targets the local PostgreSQL container (`localhost:5432`).
