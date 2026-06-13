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

# Runtime defaults — override at container start or via compose env
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# Injected by CI/CD — set at build time with --build-arg or at runtime via env
ENV APP_VERSION=dev
ENV BUILD_DATE=unknown
ENV COMMIT_SHA=

EXPOSE 8080
ENTRYPOINT ["dotnet", "AeroMes.Api.dll"]
