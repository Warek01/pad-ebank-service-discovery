FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

RUN apt update && apt install -y curl

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServiceDiscovery.csproj", "./"]
RUN dotnet restore "ServiceDiscovery.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ServiceDiscovery.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ServiceDiscovery.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 3000
HEALTHCHECK --interval=15s --timeout=5s --start-period=10s --retries=3  CMD curl --fail http://localhost:3000/healthz || exit

ENTRYPOINT ["dotnet", "ServiceDiscovery.dll"]
