#!/usr/bin/env bash
# Migrates PostgreSQL data from PG 16 (or earlier) to PG 18.
#
# PG 18 changed the Docker volume mount path:
#   PG 16 and earlier: /var/lib/postgresql/data
#   PG 18 and later:   /var/lib/postgresql  (data lives in 18/docker/)
#
# Usage: ./scripts/migrate-postgres-to-18.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
COMPOSE_FILE="$PROJECT_DIR/docker-compose.yml"
DUMP_FILE="$PROJECT_DIR/pg16_backup.dump"

PG_USER="${POSTGRES_USER:-darts}"
PG_DB="${POSTGRES_DB:-dartsdb}"
OLD_VOLUME="workspace_postgres_data"
NEW_VOLUME="workspace_postgres_data_18"

RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m'

step() { echo -e "\n${CYAN}=== $1 ===${NC}"; }
ok()   { echo -e "${GREEN}$1${NC}"; }
warn() { echo -e "${YELLOW}$1${NC}"; }
die()  { echo -e "${RED}ERROR: $1${NC}" >&2; exit 1; }

cd "$PROJECT_DIR"

if ! command -v docker >/dev/null 2>&1; then
  die "Docker is required."
fi

step "PostgreSQL 16 → 18 data migration"

# Resolve actual volume names (compose project prefix may vary)
resolve_volume() {
  local suffix="$1"
  docker volume ls --format '{{.Name}}' | grep -E "${suffix}$|_${suffix}$" | head -1
}

OLD_VOL="$(resolve_volume 'postgres_data' || true)"
NEW_VOL="$(resolve_volume 'postgres_data_18' || true)"

if [[ -z "$OLD_VOL" ]]; then
  OLD_VOL="$OLD_VOLUME"
fi
if [[ -z "$NEW_VOL" ]]; then
  NEW_VOL="$NEW_VOLUME"
fi

if ! docker volume inspect "$OLD_VOL" >/dev/null 2>&1; then
  warn "No legacy PG 16 volume found ($OLD_VOL)."
  warn "Starting fresh with PostgreSQL 18 storage layout..."
  docker compose up -d
  ok "PostgreSQL 18 started. Run the API locally: cd api/DartsApi && dotnet run"
  exit 0
fi

if docker volume inspect "$NEW_VOL" >/dev/null 2>&1; then
  PG18_SIZE="$(docker run --rm -v "$NEW_VOL:/var/lib/postgresql:ro" alpine sh -c 'du -sk /var/lib/postgresql 2>/dev/null | cut -f1' || echo 0)"
  if [[ "$PG18_SIZE" -gt 1024 ]]; then
    warn "PG 18 volume ($NEW_VOL) already contains data (${PG18_SIZE}KB)."
    read -r -p "Continue and overwrite? [y/N] " confirm
    [[ "$confirm" =~ ^[Yy]$ ]] || die "Aborted."
    docker volume rm "$NEW_VOL" 2>/dev/null || true
  fi
fi

step "1/6 Stopping Postgres (keeping data reachable for dump)"
docker compose stop postgres 2>/dev/null || true

# Start a temporary PG16 container against the old volume if not already running
PG16_CONTAINER="darts-pg16-migrate"
if ! docker ps --format '{{.Names}}' | grep -q "^${PG16_CONTAINER}$"; then
  docker rm -f "$PG16_CONTAINER" 2>/dev/null || true
  docker run -d --name "$PG16_CONTAINER" \
    -e POSTGRES_USER="$PG_USER" \
    -e POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-darts123}" \
    -e POSTGRES_DB="$PG_DB" \
    -v "$OLD_VOL:/var/lib/postgresql/data" \
    postgres:16-alpine >/dev/null

  ok "Started temporary PG 16 container for dump"
  echo -n "Waiting for PG 16"
  for _ in $(seq 1 30); do
    if docker exec "$PG16_CONTAINER" pg_isready -U "$PG_USER" -d "$PG_DB" >/dev/null 2>&1; then
      echo " ready"
      break
    fi
    echo -n "."
    sleep 1
  done
fi

step "2/6 Dumping database to $DUMP_FILE"
docker exec "$PG16_CONTAINER" pg_dump -U "$PG_USER" -Fc "$PG_DB" > "$DUMP_FILE"
DUMP_SIZE="$(du -h "$DUMP_FILE" | cut -f1)"
ok "Dump complete ($DUMP_SIZE)"

step "3/6 Stopping all containers"
docker compose down 2>/dev/null || true
docker rm -f "$PG16_CONTAINER" 2>/dev/null || true

step "4/6 Backing up legacy volume"
BACKUP_VOL="${OLD_VOL}_backup_$(date +%Y%m%d)"
if ! docker volume inspect "$BACKUP_VOL" >/dev/null 2>&1; then
  docker volume create "$BACKUP_VOL" >/dev/null
  docker run --rm \
    -v "$OLD_VOL:/from:ro" \
    -v "$BACKUP_VOL:/to" \
    alpine sh -c "cp -a /from/. /to/" >/dev/null
  ok "Legacy volume copied to $BACKUP_VOL"
else
  warn "Backup volume $BACKUP_VOL already exists, skipping copy"
fi

step "5/6 Starting PostgreSQL 18 with new storage layout"
docker compose up -d postgres

echo -n "Waiting for PG 18"
for _ in $(seq 1 30); do
  if docker exec darts-postgres pg_isready -U "$PG_USER" -d "$PG_DB" >/dev/null 2>&1; then
    echo " ready"
    break
  fi
  echo -n "."
  sleep 1
done

step "6/6 Restoring data into PostgreSQL 18"
docker exec -i darts-postgres pg_restore -U "$PG_USER" -d "$PG_DB" --clean --if-exists --no-owner < "$DUMP_FILE"
ok "Data restored"

docker compose up -d postgres
ok "Migration complete! Start the API locally: cd api/DartsApi && dotnet run"
echo ""
echo "  Dump file:     $DUMP_FILE"
echo "  Legacy backup: $BACKUP_VOL (old PG 16 volume preserved)"
echo "  New volume:    $NEW_VOL → /var/lib/postgresql (PG 18 layout)"
echo ""
warn "You can remove the legacy volume once verified:"
echo "  docker volume rm $OLD_VOL"
