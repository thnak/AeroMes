#!/usr/bin/env bash
# Zero-downtime rolling update for AeroMes HA deployment.
#
# What happens:
#   1. Pull the new image so it's cached locally.
#   2. Restart api-01 (which runs DB migrations).
#      nginx passively detects it's restarting, retries requests on api-02.
#   3. Wait for Docker's built-in health check (GET /health) to report "healthy".
#   4. Restart api-02 (already migrated; SKIP_MIGRATIONS=true).
#   5. Wait for api-02 to become healthy.
#
# Required env vars (can also come from .env in the same directory):
#   IMAGE  — Docker registry + image name, e.g. ghcr.io/your-org/aeromes
#   TAG    — image tag to deploy, e.g. v1.3.0
#
# Optional:
#   COMPOSE_FILE — path to the compose file (default: docker-compose.ha.yml)
#   APP_VERSION  — human-readable version shown in the UI (default: $TAG)
#   BUILD_DATE   — ISO 8601 date (default: current UTC time)
#   COMMIT_SHA   — git commit SHA (optional)

set -euo pipefail

COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.ha.yml}"
IMAGE="${IMAGE:?IMAGE env var is required}"
TAG="${TAG:?TAG env var is required}"
APP_VERSION="${APP_VERSION:-$TAG}"
BUILD_DATE="${BUILD_DATE:-$(date -u +'%Y-%m-%dT%H:%M:%SZ')}"
COMMIT_SHA="${COMMIT_SHA:-}"

export IMAGE TAG APP_VERSION BUILD_DATE COMMIT_SHA

log() { printf '\033[0;36m[%s]\033[0m %s\n' "$(date -u +%H:%M:%S)" "$*"; }
ok()  { printf '\033[0;32m[%s] ✓ %s\033[0m\n' "$(date -u +%H:%M:%S)" "$*"; }
err() { printf '\033[0;31m[%s] ✗ %s\033[0m\n' "$(date -u +%H:%M:%S)" "$*" >&2; }

# wait_healthy <service> [timeout_seconds]
# Polls docker inspect until the container's health status is "healthy".
wait_healthy() {
    local service=$1
    local timeout=${2:-120}
    local container_id elapsed=0

    container_id=$(docker compose -f "$COMPOSE_FILE" ps -q "$service" 2>/dev/null | head -1)
    if [ -z "$container_id" ]; then
        err "Could not find container for service: $service"
        return 1
    fi

    log "Waiting for $service (${container_id:0:12}) to become healthy (max ${timeout}s)..."
    while [ $elapsed -lt $timeout ]; do
        local status
        status=$(docker inspect --format='{{.State.Health.Status}}' "$container_id" 2>/dev/null || echo "unknown")
        case "$status" in
            healthy) ok "$service is healthy"; return 0 ;;
            unhealthy)
                err "$service health check failed — recent logs:"
                docker compose -f "$COMPOSE_FILE" logs --tail=30 "$service"
                return 1
                ;;
        esac
        sleep 3
        elapsed=$((elapsed + 3))
    done

    err "$service did not become healthy within ${timeout}s"
    docker compose -f "$COMPOSE_FILE" logs --tail=50 "$service"
    return 1
}

# ── Main ───────────────────────────────────────────────────────────────────────

log "=== AeroMes rolling deploy ==="
log "  Image : $IMAGE:$TAG"
log "  Version: $APP_VERSION"
log "  Built  : $BUILD_DATE"
[ -n "$COMMIT_SHA" ] && log "  Commit : $COMMIT_SHA"

log "Pulling $IMAGE:$TAG ..."
docker compose -f "$COMPOSE_FILE" pull api-01 api-02

# ── Step 1: api-01 (runs migrations) ──────────────────────────────────────────
log "Restarting api-01 (primary — runs DB migrations) ..."
docker compose -f "$COMPOSE_FILE" up -d --no-deps --force-recreate api-01
wait_healthy api-01

# ── Step 2: api-02 (SKIP_MIGRATIONS=true) ─────────────────────────────────────
log "Restarting api-02 ..."
docker compose -f "$COMPOSE_FILE" up -d --no-deps --force-recreate api-02
wait_healthy api-02

ok "=== Deploy complete: $IMAGE:$TAG ==="
