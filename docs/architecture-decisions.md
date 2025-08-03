# Villagers - Architecture Decisions

## Overview
This document tracks important architecture decisions and discussions for the Villagers game project.

## Current Architecture

### Independent Services Structure
- **Game Server (Port 5033)**: Authoritative game simulation with SignalR real-time communication
- **API (Port 3001)**: Authentication and player management service
- **Frontend (Port 3000)**: React TypeScript client

### Communication Flow
```
Client ──HTTP──► API (Auth/Player Management)
   │
   └──────────SignalR──────────► Game Server (Commands & Updates)
```

## Key Decisions

### 1. SignalR-Based Command Architecture ✅ **IMPLEMENTED**

**Current Implementation:**
- Clients connect directly to Game Server via SignalR for real-time commands
- Game Server maintains in-memory command queue with immediate processing
- Commands use Guid-based player IDs for consistency
- No HTTP API layer for game commands (removed for better performance)

**Benefits:**
- True real-time command execution
- Eliminates API-to-GameServer HTTP overhead
- Simplified architecture with fewer moving parts
- Better scalability for real-time game interactions

**Implementation Details:**
```csharp
// Direct SignalR command flow
1. Client ──SignalR──► Game Server: Send command via hub
2. Game Server: Queue command in memory
3. Game Server: Process commands on each tick
4. Game Server ──SignalR──► Clients: Broadcast updates
```

### 2. Authentication Strategy ✅ **IMPLEMENTED**

**Current Implementation:**
- Microsoft Identity with custom PlayerEntity (inherits IdentityUser<Guid>)
- Username-only authentication (no email requirement)
- JWT token-based authentication with claims
- Token validation and refresh endpoints
- Domain-driven architecture with Entity/Domain/Model separation

**Implementation Details:**
```csharp
// Authentication flow
1. Client ──POST──► API: /api/auth/login (username only)
2. API: Validate user, generate JWT token
3. API ──Response──► Client: JWT token + player info
4. Client: Use JWT for API calls, player ID for game server
```

**Benefits:**
- Secure token-based authentication
- Separation of auth (API) and game logic (Game Server)
- Scalable with standard Identity patterns

### 3. Game State Distribution

**Challenge:** Sending full game state to all players is costly

**Proposed Solutions:**
1. **Area of Interest (AOI)**: Only send data for nearby villages
2. **Delta Updates**: Send only changes, not full state
3. **Subscription-Based**: Players subscribe to specific regions
4. **Distance-Based Frequency**: Update frequency decreases with distance

**For Villagers Specifically:**
- Full state for player's own villages
- Viewport-based updates for map
- Public info only for distant villages

## Database Strategy ✅ **IMPLEMENTED**

### Domain-Driven Database Architecture
- **API Database**: PostgreSQL with player authentication and management
- **Game Server Database**: PostgreSQL with world state and command persistence
- **Domain-Centric Repositories**: Repositories work with domain objects, not entities
- **Entity Framework Core**: Code-first approach with migrations

### Repository Pattern Implementation
```csharp
// Domain-centric approach
1. Repositories expose domain objects (Player, World, Command)
2. Internal entity-to-domain conversion via constructor chaining
3. Clean separation: Entity (DB) ↔ Domain (Business Logic) ↔ Model (API)
```

### Benefits
- Clean domain boundaries
- Testable business logic
- Database independence through abstractions
- Configuration-driven development

## Major Architectural Changes

### 4. Shared Project Removal ✅ **IMPLEMENTED**
**Decision**: Removed shared project dependency between API and Game Server
- Each service now has independent domain models and entities
- Eliminates coupling between services
- Allows independent development and deployment
- Maintains clean service boundaries

### 5. Test Structure Standardization ✅ **IMPLEMENTED**
**Decision**: Standardized test projects to use `src/` directory structure
- `api.tests/src/` and `game-server.tests/src/` mirror main project structure
- Consistent organization across all projects
- Better maintainability and navigation

## Current Project Structure
```
/
├── api/                           # Authentication & Player Management
│   ├── src/Domain/               # Domain models (Player)
│   ├── src/Entities/             # Database entities (PlayerEntity)  
│   ├── src/Controllers/          # API controllers (Auth)
│   └── src/Data/                 # DbContext and configuration
├── game-server/                  # Game Simulation & Real-time Communication
│   ├── src/Domain/               # Domain models (World, Commands)
│   ├── src/Entities/             # Database entities
│   ├── src/Infrastructure/       # Repository implementations
│   ├── src/Hubs/                 # SignalR hubs
│   └── src/Services/             # Game simulation service
├── api.tests/src/                # API tests with src structure
├── game-server.tests/src/        # Game Server tests with src structure
└── frontend/                     # React TypeScript client
```

## Open Questions

1. **Game Content**: What specific game mechanics to implement first?
   - Village building system
   - Resource management
   - Player interactions

2. **Scalability**: How to handle multiple concurrent worlds?

3. **Persistence Strategy**: How often should world state be persisted?

4. **Event Sourcing**: Consider for future iterations?

## Next Steps

1. Implement core game mechanics (villages, resources)
2. Add viewport-based updates for efficient client synchronization  
3. Implement proper world/village management
4. Add comprehensive integration tests
5. Performance optimization and monitoring