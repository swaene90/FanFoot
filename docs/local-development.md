# Local Development

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Setup

**1. Start the local database**

```powershell
docker compose --profile local up -d --wait db
```

This starts only the PostgreSQL 17 container on `localhost:5432` with the persistent `fanfoot-db-data` volume. The `web` service is for the production-style Docker workflow and is not needed when running the host with `dotnet run`.

**2. Build the React client and run the app**

```powershell
cd src/Fanfoot.Web
npm ci --prefix ClientApp
npm run build --prefix ClientApp
dotnet run
```

App runs at http://localhost:5020.

Run `npm run dev --prefix ClientApp` in a separate terminal when iterating on the React UI. The production host serves the latest `npm run build` output from `wwwroot`.

## First Run

On first startup the app will automatically:

1. Apply all pending EF Core migrations
2. Seed the database with:
   - A local user (`admin@local.dev` / `password`)
   - A dev league
   - All NFL player data from `src/Fanfoot.Web/localPlayerData.json`

Seeding is idempotent — subsequent restarts skip it if data already exists.

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
