# Use Alpine-based .NET runtime (much smaller)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# Use Alpine SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Subtitles.Api/Subtitles.Api.csproj", "Subtitles.Api/"]
RUN dotnet restore "Subtitles.Api/Subtitles.Api.csproj" \
    --runtime alpine-x64 \
    --no-cache

# Copy source code
COPY . .
WORKDIR "/src/Subtitles.Api"

# Publish with aggressive optimizations
RUN dotnet publish "Subtitles.Api.csproj" \
    -c Release \
    -o /app/publish \
    --runtime alpine-x64 \
    --self-contained false \
    --no-restore \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# Final stage - use minimal Alpine runtime
FROM base AS final
WORKDIR /app

# Install required packages and create user in one layer
RUN apk add --no-cache \
        ca-certificates \
        tzdata \
    && adduser -D -s /bin/sh appuser \
    && mkdir -p /tmp/serilog-buffer \
    && chown -R appuser:appuser /app /tmp/serilog-buffer

# Copy published app
COPY --from=build /app/publish .

# Set non-root user
USER appuser

ENTRYPOINT ["dotnet", "Subtitles.Api.dll"]
