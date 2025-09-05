# Use the official .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Subtitles.Api/Subtitles.Api.csproj", "Subtitles.Api/"]
RUN dotnet restore "Subtitles.Api/Subtitles.Api.csproj"
COPY . .
WORKDIR "/src/Subtitles.Api"
RUN dotnet build "Subtitles.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Subtitles.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create directories for Serilog buffering
RUN mkdir -p /tmp/serilog-buffer && chmod 777 /tmp/serilog-buffer

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app /tmp/serilog-buffer
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Subtitles.Api.dll"]
