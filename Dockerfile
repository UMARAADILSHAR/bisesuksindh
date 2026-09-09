# Multi-Stage Production Dockerfile for BISE Hyderabad Portal

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project definitions for optimized caching
COPY BiseHyderabad.slnx ./
COPY src/BiseHyderabad.Core/BiseHyderabad.Core.csproj src/BiseHyderabad.Core/
COPY src/BiseHyderabad.Application/BiseHyderabad.Application.csproj src/BiseHyderabad.Application/
COPY src/BiseHyderabad.Infrastructure/BiseHyderabad.Infrastructure.csproj src/BiseHyderabad.Infrastructure/
COPY src/BiseHyderabad.Web/BiseHyderabad.Web.csproj src/BiseHyderabad.Web/
COPY tests/BiseHyderabad.Core.Tests/BiseHyderabad.Core.Tests.csproj tests/BiseHyderabad.Core.Tests/
COPY tests/BiseHyderabad.Infrastructure.Tests/BiseHyderabad.Infrastructure.Tests.csproj tests/BiseHyderabad.Infrastructure.Tests/
COPY tests/BiseHyderabad.Web.Tests/BiseHyderabad.Web.Tests.csproj tests/BiseHyderabad.Web.Tests/

RUN dotnet restore BiseHyderabad.slnx

# Copy full repository source
COPY . .

# Build and Publish Web Application
WORKDIR /src/src/BiseHyderabad.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

LABEL maintainer="BISE Hyderabad Dev Team"
LABEL description="BISE Hyderabad Board Management Portal"
LABEL org.opencontainers.image.source="https://github.com/bise-hyderabad/portal"

WORKDIR /app

# Set Production Environment Flags
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

RUN chown -R 1654:1654 /app

# ECS-native container health check (liveness probe)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1

# Run with non-root security container profile (dotnet default UID)
USER 1654

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet BiseHyderabad.Web.dll"]
