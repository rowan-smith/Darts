#!/usr/bin/env bash
# Run API end-to-end tests.
# Uses Testcontainers (Docker) when available, otherwise local Postgres on dartsdb_test.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='dartsdb_test'" 2>/dev/null | grep -q 1; then
  echo "Creating local test database dartsdb_test (fallback when Docker is unavailable)..."
  sudo -u postgres psql -c "CREATE DATABASE dartsdb_test OWNER darts;" 2>/dev/null || \
    sudo -u postgres psql -c "CREATE USER darts WITH PASSWORD 'darts123'; CREATE DATABASE dartsdb_test OWNER darts;" 2>/dev/null || true
fi

cd "$ROOT_DIR/api/DartsApi.E2E.Tests"
dotnet test --verbosity normal "$@"
