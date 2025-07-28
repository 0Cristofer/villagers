# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a village-building strategy game called "Villagers" built for backend programming practice using a microservices architecture:
- **Game Server (ECS)**: Authoritative game simulation with SignalR for real-time updates
- **Lambda API**: Stateless HTTP API for client commands
- **Frontend**: React with TypeScript client

## Architecture

```
Client ──HTTP──► Lambda API ──HTTP──► Game Server (ECS)
   │                                        │
   └──────────SignalR (real-time)──────────┘
```

**Flow:**
1. Client sends commands to Lambda API (build, recruit, attack)
2. Lambda API forwards commands to Game Server via HTTP
3. Game Server processes commands on game ticks
4. Game Server sends real-time updates to clients via SignalR

## Project Structure

```
/
├── Villagers.sln               # Solution file
├── game-server/                # ECS Game Server (.NET 8)
│   ├── Controllers/            # HTTP endpoints for receiving commands
│   ├── Services/               # Game simulation service
│   ├── Hubs/                   # SignalR hubs for real-time updates
│   └── Interfaces/             # Strongly-typed SignalR interfaces
├── api/                        # Lambda API (.NET 8)
│   ├── Controllers/            # REST API controllers
│   └── Services/               # Game server communication
└── frontend/                   # React + TypeScript client
    ├── src/types/              # TypeScript interfaces
    └── src/                    # React components
```

## Development Commands

### Quick Start (All Services)
```bash
# Start all services (builds first, then starts)
./start-dev.sh

# Stop all services
./stop-dev.sh
```

### Individual Services

#### Game Server (Port 5033)
```bash
cd game-server && dotnet run
cd game-server && dotnet build
```

#### Lambda API (Port 3001)
```bash
cd api && dotnet run --urls="http://localhost:3001"
cd api && dotnet build
```

#### Frontend (Port 3000)
```bash
cd frontend && npm start
cd frontend && npm run build
```

## Development URLs

- **Frontend**: http://localhost:3000
- **Lambda API**: http://localhost:3001
- **Lambda API Swagger**: http://localhost:3001/swagger  
- **Game Server**: http://localhost:5033

## API Examples

### Build a Building
```bash
POST http://localhost:3001/api/game/village/1/build
Content-Type: application/json

{
  "buildingType": "Barracks"
}
```

### Recruit Troops
```bash
POST http://localhost:3001/api/game/village/1/recruit
Content-Type: application/json

{
  "troopType": "Spearman",
  "quantity": 5
}
```

## Architecture Benefits

- **Scalable**: Game Server and API can scale independently
- **Resilient**: API failures don't affect game simulation
- **Real-time**: SignalR provides immediate updates to players
- **Stateless API**: Perfect for Lambda deployment
- **Authoritative Server**: Game Server maintains single source of truth

## Git Workflow

This project follows **Git Flow** branching strategy:

### Branch Structure
- **`main`** - Latest stable public release (production-ready)
- **`develop`** - Active development branch (integration branch)
- **`feature/*`** - Feature branches for new functionality

### Workflow Process

**Feature Development:**
1. Create feature branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/your-feature-name
   ```

2. Develop your feature with regular commits

3. Merge back to `develop` with **no fast-forward**:
   ```bash
   git checkout develop
   git pull origin develop
   git merge --no-ff feature/your-feature-name
   git branch -d feature/your-feature-name
   ```

**Release Process:**
When ready for a new release:
```bash
git checkout main
git pull origin main
git merge --no-ff develop
git tag -a v1.x.x -m "Release version 1.x.x"
git push origin main --tags
```

### Branch Protection
- **`main`**: Protected, only accepts merges from `develop`
- **`develop`**: Integration branch for all features
- **Feature branches**: Short-lived, deleted after merge

This ensures clean history and stable releases while allowing parallel feature development.