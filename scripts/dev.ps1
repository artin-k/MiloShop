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

    # Check if SQL Server is running on localhost
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = "Server=localhost;Integrated Security=true;Connection Timeout=3;"
    try {
        $sqlConnection.Open()
        $sqlConnection.Close()
    } catch {
        throw "SQL Server is not running on localhost. Please start SQL Server and try again."
    }

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
