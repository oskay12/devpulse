# syntax=docker/dockerfile:1
# Build context must be the repository root, e.g.:
#   docker build -f src/DevPulse.Worker/Dockerfile -t devpulse-worker .

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy the solution file plus every .csproj first (including projects the
# Worker doesn't reference, e.g. Api) so the whole dependency graph in the
# .sln restores together and stays cached independently of source changes.
COPY ["DevPulse.sln", "./"]
COPY ["src/DevPulse.Worker/DevPulse.Worker.csproj", "src/DevPulse.Worker/"]
COPY ["src/DevPulse.Core/DevPulse.Core.csproj", "src/DevPulse.Core/"]
COPY ["src/DevPulse.Infrastructure/DevPulse.Infrastructure.csproj", "src/DevPulse.Infrastructure/"]
COPY ["src/DevPulse.Api/DevPulse.Api.csproj", "src/DevPulse.Api/"]
RUN dotnet restore "DevPulse.sln"

COPY . .

# No --no-restore: the source copy above can't invalidate anything since
# csproj files didn't change, but publish re-checking guards against a
# restore that becomes stale for any reason.
RUN dotnet publish "src/DevPulse.Worker/DevPulse.Worker.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Final stage ----
# Worker has no HTTP listener, so the slimmer runtime image (no ASP.NET
# hosting bits) is enough.
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S devpulse && adduser -S devpulse -G devpulse
USER devpulse

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DevPulse.Worker.dll"]
