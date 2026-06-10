#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_DIR="$ROOT_DIR/src/Fanfoot.Web"

RESET=false
for arg in "$@"; do
  [[ "$arg" == "--reset" ]] && RESET=true
done

# Stop any running instance
if pgrep -f "Fanfoot.Web" > /dev/null 2>&1; then
  echo "Stopping existing Fanfoot.Web process..."
  pkill -f "Fanfoot.Web"
  sleep 0.5
fi

echo "Starting local database..."
docker compose --profile local up -d --wait

if [[ "$RESET" == true ]]; then
  echo "Dropping database..."
  ASPNETCORE_ENVIRONMENT=Development dotnet ef database drop --force --project "$WEB_DIR"
  echo "Database dropped — migrations and seed will run on startup."
fi

echo "Starting Fanfoot.Web at http://localhost:5020"
cd "$WEB_DIR"
dotnet run
