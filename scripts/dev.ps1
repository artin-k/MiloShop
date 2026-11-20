[CmdletBinding()]
param(
    [switch]$NoRun
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Assert-LastExitCode {
    param([string]$Operation)

    if ($LASTEXITCODE -ne 0) {
        throw "$Operation failed with exit code $LASTEXITCODE."
    }
}

Push-Location $repositoryRoot

try {
    $sdkVersion = dotnet --version
    if ($LASTEXITCODE -ne 0 -or -not $sdkVersion.StartsWith("11.")) {
        throw "Install the .NET 11 SDK. This repository requires SDK 11.0.100 or a compatible patch."
    }

    docker info *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Start Docker Desktop and wait until its Linux container engine reports that it is running."
    }

    if ([string]::IsNullOrWhiteSpace($env:MSSQL_SA_PASSWORD)) {
        throw 'Set $env:MSSQL_SA_PASSWORD to a strong local development password before running this script.'
    }

    docker compose up -d sqlserver
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose could not start the MiloShop SQL Server container."
    }

    $databaseReady = $false
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        $health = docker inspect --format "{{.State.Health.Status}}" miloshop-sql 2>$null
        if ($health -eq "healthy") {
            $databaseReady = $true
            break
        }

        Start-Sleep -Seconds 2
    }

    if (-not $databaseReady) {
        docker compose logs --tail 50 sqlserver
        throw "SQL Server did not become healthy within the expected time."
    }

    $env:ConnectionStrings__DefaultConnection = "Server=localhost,14333;Database=MiloShopLocal;User ID=sa;Password=$env:MSSQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"

    dotnet tool restore
    Assert-LastExitCode "Restoring local .NET tools"
    dotnet restore MiloShop.sln
    Assert-LastExitCode "Restoring NuGet packages"
    dotnet build MiloShop.sln --no-restore
    Assert-LastExitCode "Building MiloShop"
    dotnet test tests/MiloShop.Tests/MiloShop.Tests.csproj --no-build
    Assert-LastExitCode "Running tests"
    dotnet ef database update --project src/MiloShop.Infrastructure/MiloShop.Infrastructure.csproj --startup-project src/MiloShop.Web/MiloShop.Web.csproj
    Assert-LastExitCode "Applying database migrations"

    if (-not $NoRun) {
        dotnet run --project src/MiloShop.Web/MiloShop.Web.csproj --no-build
        Assert-LastExitCode "Running MiloShop"
    }
}
finally {
    Pop-Location
}
