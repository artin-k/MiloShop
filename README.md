# MiloShop

MiloShop is an ASP.NET Core 11 storefront organized as a clean, layered solution.

## Projects

- `MiloShop.Domain` contains business entities and has no framework dependencies.
- `MiloShop.Application` contains use-case and service contracts.
- `MiloShop.Infrastructure` contains Entity Framework Core, SQL Server, Identity, migrations, and persistence services.
- `MiloShop.Web` is the MVC presentation and composition layer.

Dependencies point inward from Web and Infrastructure to Application and Domain.

## Local development

```bash
dotnet restore MiloShop.sln
dotnet build MiloShop.sln
dotnet run --project src/MiloShop.Web/MiloShop.Web.csproj
```

Set `ConnectionStrings__DefaultConnection` to a SQL Server connection string before starting the application. Apply existing migrations with:

```bash
dotnet ef database update \
  --project src/MiloShop.Infrastructure/MiloShop.Infrastructure.csproj \
  --startup-project src/MiloShop.Web/MiloShop.Web.csproj
```
