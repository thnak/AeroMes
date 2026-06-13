# ── Stage 1: Build frontend ────────────────────────────────────────────────────
FROM node:22-alpine AS frontend-builder
WORKDIR /repo
COPY web/package*.json ./web/
RUN cd web && npm ci --prefer-offline
COPY web/ ./web/
# Vite outDir is ../src/AeroMes.Api/wwwroot (relative to web/) — create it first
RUN mkdir -p ./src/AeroMes.Api/wwwroot && cd web && npm run build

# ── Stage 2: Build backend ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-builder
WORKDIR /repo
COPY src/ ./src/
# Bring in the compiled SPA from stage 1
COPY --from=frontend-builder /repo/src/AeroMes.Api/wwwroot ./src/AeroMes.Api/wwwroot
RUN dotnet publish src/AeroMes.Api/AeroMes.Api.csproj \
    -c Release \
    -o /publish \
    --no-self-contained

# ── Stage 3: Runtime image ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend-builder /publish .

# Version baked at build time via --build-arg; can still be overridden at runtime via -e
ARG APP_VERSION=dev
ARG BUILD_DATE=unknown
ARG COMMIT_SHA=
ENV APP_VERSION=$APP_VERSION
ENV BUILD_DATE=$BUILD_DATE
ENV COMMIT_SHA=$COMMIT_SHA

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
# HEALTHCHECK uses wget (included in the debian-based aspnet base image)
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -qO /dev/null http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AeroMes.Api.dll"]
