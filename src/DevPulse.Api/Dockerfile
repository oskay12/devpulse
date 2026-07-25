# syntax=docker/dockerfile:1
# Build context must be the repository root, e.g.:
#   docker build -f src/DevPulse.Api/Dockerfile -t devpulse-api .

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy the solution file plus every .csproj first (including projects the
# Api doesn't reference, e.g. Worker) so the whole dependency graph in the
# .sln restores together and stays cached independently of source changes.
COPY ["DevPulse.sln", "./"]
COPY ["src/DevPulse.Api/DevPulse.Api.csproj", "src/DevPulse.Api/"]
COPY ["src/DevPulse.Core/DevPulse.Core.csproj", "src/DevPulse.Core/"]
COPY ["src/DevPulse.Infrastructure/DevPulse.Infrastructure.csproj", "src/DevPulse.Infrastructure/"]
COPY ["src/DevPulse.Worker/DevPulse.Worker.csproj", "src/DevPulse.Worker/"]
RUN dotnet restore "DevPulse.sln"

COPY . .

# No --no-restore: the source copy above can't invalidate anything since
# csproj files didn't change, but publish re-checking guards against a
# restore that becomes stale for any reason.
RUN dotnet publish "src/DevPulse.Api/DevPulse.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Final stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S devpulse && adduser -S devpulse -G devpulse
USER devpulse

# ICU/globalization data isn't shipped on alpine; the API doesn't need
# culture-specific formatting, so invariant mode avoids pulling icu-libs.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DevPulse.Api.dll"]
