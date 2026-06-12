# PDC Darts Tracker

A full-stack dart scoring application inspired by the Professional Darts Corporation (PDC). Track live 501 matches, browse news and featured content, and manage your player profile — all from your mobile device via Expo Go.

## Architecture

```
├── api/          ASP.NET Core 10 Web API + Entity Framework Core
├── client/       React Native (Expo SDK 54) mobile app
└── docker-compose.yml   PostgreSQL 18 Alpine + API containers
```

## Features

- **Home** — Recent scores, featured highlights, latest news articles, and practice suggestions
- **Scores** — Create, start, score, and filter multiple matches (Scheduled / Live / Completed)
- **Match Scoring** — PDC-style 501 scoring with bust/checkout validation, quick-score pad, and leg/set tracking
- **Match Recap** — Read-only summary after a match completes (averages, 180s, set/leg breakdown)
- **Profile** — Edit name/email, toggle light/dark mode, view personal stats
- **Seed Data** — Rich example data loaded automatically on first API start

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (for containerised Postgres + API)
- [Expo Go 54](https://expo.dev/go) on your physical device

## Quick Start

### 1. Start the database and API (Docker)

```bash
docker compose up -d
```

The API will be available at `http://localhost:8080`. Swagger UI: `http://localhost:8080/swagger`.

### 2. Start the API locally (alternative)

If you have PostgreSQL running locally:

```bash
# Create database user (one-time)
psql -U postgres -c "CREATE USER darts WITH PASSWORD 'darts123';"
psql -U postgres -c "CREATE DATABASE dartsdb OWNER darts;"

cd api/DartsApi
dotnet run
```

Connection string (default): `Host=localhost;Port=5432;Database=dartsdb;Username=darts;Password=darts123`

### 3. Start the mobile client

```bash
cd client
npm install          # .npmrc handles Expo peer dependency resolution
cp .env.example .env
# Edit .env — set EXPO_PUBLIC_API_BASE_URL to your machine's LAN IP:
#   EXPO_PUBLIC_API_BASE_URL=http://192.168.1.100:8080
npx expo start
# or clear Metro cache after config changes:
npm run start:clear
```

The client uses **Metro** as the bundler (see `client/metro.config.js`). `expo start` runs Metro under the hood for iOS, Android, and web.

Configure the API in `client/.env`:

```env
EXPO_PUBLIC_API_BASE_URL=http://192.168.1.100:8080
```

The `/api` path is appended automatically. Restart Expo after changing `.env`. In dev, the resolved URL is logged as `[API] Base URL: ...`.

Scan the QR code with **Expo Go 54** on your phone.

> **Physical device tip:** Phone and computer must be on the same Wi-Fi. Ensure the API is running (`docker compose up -d` or `dotnet run`). If the home screen shows "Cannot reach API", verify `EXPO_PUBLIC_API_BASE_URL` matches your machine's IP — not `localhost`.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Health check (used by client on startup) |
| GET | `/api/home` | Home feed (scores, articles, suggestions, featured) |
| GET | `/api/matches` | List matches (optional `?status=InProgress`) |
| GET | `/api/matches/{id}` | Match detail with sets/legs/visits |
| GET | `/api/matches/{id}/recap` | Completed match recap (read-only) |
| POST | `/api/matches` | Create a new match |
| POST | `/api/matches/{id}/start` | Start a scheduled match |
| POST | `/api/matches/{id}/visit` | Record a scoring visit |
| GET | `/api/players` | List all players |
| GET/PUT | `/api/profile` | Get/update user profile |
| GET | `/api/articles` | List news articles |

## Tech Stack

| Layer | Technology |
|-------|-----------|
| API | ASP.NET Core 10, EF Core, PostgreSQL |
| Database | PostgreSQL 18 Alpine |
| Mobile | React Native, Expo SDK 54, Expo Router |
| Containers | Docker Alpine (Postgres + .NET runtime) |

## PostgreSQL 18 storage

PostgreSQL 18 changed the Docker volume mount path. This project uses the PG 18 layout:

| Version | Image | Volume mount |
|---------|-------|--------------|
| 16 and earlier | `postgres:16-alpine` | `/var/lib/postgresql/data` |
| **18 (current)** | `postgres:18-alpine` | `/var/lib/postgresql` |

Data is stored under `18/docker/` inside the `postgres_data_18` volume automatically.

### Migrating from PostgreSQL 16

If you have an existing PG 16 `postgres_data` volume, run the migration script:

```bash
chmod +x scripts/migrate-postgres-to-18.sh
./scripts/migrate-postgres-to-18.sh
```

The script will:
1. Dump your PG 16 database
2. Back up the legacy Docker volume
3. Start PG 18 with the new storage path
4. Restore your data
5. Bring the full stack back up

For a **fresh install**, just run `docker compose up -d` — no migration needed.

## Clearing Seed Data

Seed data is inserted only when the `Players` table is empty. To reset:

```bash
docker compose down -v   # removes Postgres 18 volume
docker compose up -d     # fresh database with new seed data
```

Or drop and recreate the local database, then restart the API.
