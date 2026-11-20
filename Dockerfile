FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MiloShop.Domain/MiloShop.Domain.csproj", "src/MiloShop.Domain/"]
COPY ["src/MiloShop.Application/MiloShop.Application.csproj", "src/MiloShop.Application/"]
COPY ["src/MiloShop.Infrastructure/MiloShop.Infrastructure.csproj", "src/MiloShop.Infrastructure/"]
COPY ["src/MiloShop.Web/MiloShop.Web.csproj", "src/MiloShop.Web/"]
RUN dotnet restore "src/MiloShop.Web/MiloShop.Web.csproj"
COPY . .
RUN dotnet build "src/MiloShop.Web/MiloShop.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/MiloShop.Web/MiloShop.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiloShop.Web.dll"]
