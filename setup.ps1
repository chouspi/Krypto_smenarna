param(
    [switch]$SkipDocker
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$envPath = Join-Path $projectRoot ".env"
$envExamplePath = Join-Path $projectRoot ".env.example"
$containerName = "kryptoSmenarnaOracleDB"

function Read-EnvFile($path) {
    $values = @{}

    Get-Content $path | ForEach-Object {
        $line = $_.Trim()

        if ($line.Length -eq 0 -or $line.StartsWith("#")) {
            return
        }

        $parts = $line.Split("=", 2)

        if ($parts.Length -eq 2) {
            $values[$parts[0].Trim()] = $parts[1].Trim()
        }
    }

    return $values
}

function Get-InitScriptOrder($fileName) {
    if ($fileName -match "^(\d+)_") {
        return [int]$matches[1]
    }

    if ($fileName -match "^F(\d+)_") {
        return [int]$matches[1]
    }

    return 999
}

function Wait-Oracle($connectionString) {
    Write-Host "Waiting for Oracle database..."

    for ($i = 1; $i -le 60; $i++) {
        $sql = "SET HEADING OFF`nSET FEEDBACK OFF`nSELECT 1 FROM dual;`nEXIT`n"
        $output = $sql | docker exec -i $containerName sqlplus -s $connectionString 2>&1

        if ($LASTEXITCODE -eq 0 -and ($output -match "1")) {
            Write-Host "Oracle database is ready."
            return
        }

        Start-Sleep -Seconds 5
    }

    throw "Oracle database did not become ready in time."
}

function Invoke-InitScript($scriptFile, $connectionString) {
    $containerScriptPath = "/container-entrypoint-initdb.d/$($scriptFile.Name)"
    $sql = "SET DEFINE OFF`nWHENEVER SQLERROR EXIT SQL.SQLCODE`n@$containerScriptPath`nEXIT`n"
    $output = $sql | docker exec -i $containerName sqlplus -s $connectionString 2>&1

    if ($LASTEXITCODE -ne 0) {
        if ($scriptFile.Name -eq "01_dbInit.sql" -and ($output -match "ORA-00955|ORA-01430|ORA-02264")) {
            Write-Host "Skipped 01_dbInit.sql because schema objects already exist."
            return
        }

        throw "Failed to run $($scriptFile.Name):`n$output"
    }

    Write-Host "Ran $($scriptFile.Name)."
}

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

$envValues = Read-EnvFile $envPath
$appUser = $envValues["APP_USER"]
$appPassword = $envValues["APP_USER_PASSWORD"]

if ([string]::IsNullOrWhiteSpace($appUser) -or [string]::IsNullOrWhiteSpace($appPassword)) {
    throw ".env must contain APP_USER and APP_USER_PASSWORD."
}

$connectionString = "$appUser/$appPassword@localhost:1521/FREEPDB1"

Push-Location $projectRoot
try {
    docker compose up -d

    Wait-Oracle $connectionString

    Get-ChildItem (Join-Path $projectRoot "db/init") -Filter "*.sql" |
        Sort-Object @{ Expression = { Get-InitScriptOrder $_.Name } }, Name |
        ForEach-Object { Invoke-InitScript $_ $connectionString }
}
finally {
    Pop-Location
}
