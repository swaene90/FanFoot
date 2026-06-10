param(
    [switch]$Reset
)

$ErrorActionPreference = "Stop"

$rootDir = Split-Path $PSScriptRoot -Parent
$webDir  = Join-Path $rootDir "src\Fanfoot.Web"

# Stop any running instance
$existing = Get-Process -Name "Fanfoot.Web" -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Stopping existing Fanfoot.Web process..."
    $existing | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

# Ensure local postgres container is running
Write-Host "Starting local database..."
docker compose --profile local up -d --wait
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($Reset) {
    Write-Host "Dropping database..."
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet ef database drop --force --project $webDir
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "Database dropped — migrations and seed will run on startup."
}

# Start the app (migrations + seed run automatically on startup)
Write-Host "Starting Fanfoot.Web at http://localhost:5020"
Set-Location $webDir
dotnet run
