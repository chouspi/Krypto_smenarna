param(
    [switch]$SkipDocker
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$envPath = Join-Path $projectRoot ".env"
$envExamplePath = Join-Path $projectRoot ".env.example"

if (-not (Test-Path $envPath)) {
    if (-not (Test-Path $envExamplePath)) {
        throw ".env.example was not found."
    }

    Copy-Item $envExamplePath $envPath
    Write-Host ".env was created from .env.example."
}
else {
    Write-Host ".env already exists."
}

if ($SkipDocker) {
    Write-Host "Docker Compose startup skipped."
    exit 0
}

Push-Location $projectRoot
try {
    docker compose up -d
}
finally {
    Pop-Location
}
