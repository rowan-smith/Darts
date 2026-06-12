# PDC Darts Tracker

A full-stack dart scoring application inspired by the Professional Darts Corporation (PDC). Track live 501 matches, browse news and featured content, and manage your player profile — all from your mobile device via Expo Go.

## Architecture

```
├── api/          ASP.NET Core 8 Web API + Entity Framework Core
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

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
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
npm install
npx expo start
```

Scan the QR code with **Expo Go 54** on your phone. The app auto-detects your machine's IP for the API (`http://<your-ip>:8080/api`).

> **Physical device tip:** Ensure your phone and computer are on the same Wi-Fi network. If the API is unreachable, update `client/constants/config.ts` with your machine's LAN IP.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
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
| API | ASP.NET Core 8, EF Core, PostgreSQL |
| Database | PostgreSQL 18 Alpine |
| Mobile | React Native, Expo SDK 54, Expo Router |
| Containers | Docker Alpine (Postgres + .NET runtime) |

## Clearing Seed Data

Seed data is inserted only when the `Players` table is empty. To reset:

```bash
docker compose down -v   # removes Postgres volume
docker compose up -d     # fresh database with new seed data
```

Or drop and recreate the local database, then restart the API.
