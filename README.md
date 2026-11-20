# MiloShop

MiloShop is an ASP.NET Core 11 storefront organized as a clean, layered solution.

## Projects

- `MiloShop.Domain` contains business entities.
- `MiloShop.Application` contains use-case and service contracts.
- `MiloShop.Infrastructure` contains EF Core, SQL Server, Identity, migrations, and persistence services.
- `MiloShop.Web` contains the MVC interface and application startup.
- `MiloShop.Tests` contains the automated test suite.

## Requirements

- .NET SDK 11.0.100 or a compatible .NET 11 patch
- Docker Desktop configured to use Linux containers
- PowerShell 5.1 or newer on Windows

The repository's `global.json` selects the required SDK. Verify it with:

```powershell
dotnet --version
```

## Windows development setup

Open Docker Desktop and wait for its engine to finish starting. From PowerShell in the repository root, set a strong local SQL Server password and run the development launcher:

```powershell
$env:MSSQL_SA_PASSWORD = "Choose-A-Strong-Local-Password123!"
powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1
```

The launcher performs these operations:

1. verifies .NET 11 and Docker Desktop
2. starts SQL Server with Docker Compose
3. restores tools and NuGet packages
4. builds the solution and runs all tests
5. applies EF Core migrations
6. starts the web application

Use `-NoRun` to prepare and validate the environment without starting the web server:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1 -NoRun
```

Stop the local database with:

```powershell
docker compose stop sqlserver
```

Start it again with:

```powershell
docker compose up -d sqlserver
```

## Manual PowerShell commands

PowerShell uses the backtick character for multiline commands. Keeping commands on one line avoids shell-specific continuation syntax:

```powershell
$env:MSSQL_SA_PASSWORD = "Choose-A-Strong-Local-Password123!"
docker compose up -d sqlserver
$env:ConnectionStrings__DefaultConnection = "Server=localhost,14333;Database=MiloShopLocal;User ID=sa;Password=$env:MSSQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"
dotnet tool restore
dotnet restore MiloShop.sln
dotnet build MiloShop.sln --no-restore
dotnet test tests/MiloShop.Tests/MiloShop.Tests.csproj --no-build
dotnet ef database update --project src/MiloShop.Infrastructure/MiloShop.Infrastructure.csproj --startup-project src/MiloShop.Web/MiloShop.Web.csproj
dotnet run --project src/MiloShop.Web/MiloShop.Web.csproj --no-build
```
