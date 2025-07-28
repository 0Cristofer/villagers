# Villagers - Architecture Decisions

## Overview
This document tracks important architecture decisions and discussions for the Villagers game project.

## Current Architecture

### Microservices Structure
- **Game Server (Port 5033)**: Authoritative game simulation with ECS pattern
- **Lambda API (Port 3001)**: Stateless HTTP API for client commands
- **Frontend (Port 3000)**: React TypeScript client

### Communication Flow
```
Client ──HTTP──► Lambda API ──HTTP──► Game Server (ECS)
   │                                        │
   └──────────SignalR (real-time)──────────┘
```

## Key Decisions

### 1. Command Queue Architecture (Under Discussion)

**Proposed Architecture:**
- Game Server maintains in-memory command queue
- Commands are read at the start of each tick
- API calls server to schedule commands, receives tick number for execution
- API persists commands with tick number to database
- Game state persisted every N ticks/seconds
- On crash recovery: replay commands from last persisted state

**Benefits:**
- Game loop never blocks on DB operations
- Predictable command execution order
- Crash recovery via command replay
- Clean separation of concerns

**Implementation Details:**
```csharp
// Command flow
1. Client → API: Send command
2. API → Game Server: Queue command, get tick number
3. API → Database: Persist command with tick number
4. Game Server: Execute commands at designated tick
5. Game Server → Database: Periodic state persistence
6. API → Database: Clear old commands after state persistence
```

### 2. Authentication Strategy

**Current Implementation:**
- Simple username-based login (no password)
- No session persistence for development
- Login just returns player info, doesn't affect game state

**Future Enhancement:**
- Login remains fast (no game server dependency)
- Village creation as separate command in queue
- Proper session management with JWT tokens

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

## Database Strategy (To Be Implemented)

### Game Server Database Approach
1. **Load on startup**: Read entire game state into memory
2. **In-memory operations**: All game logic uses memory objects
3. **Periodic saves**: Persist every 30 seconds (configurable)
4. **Immediate saves**: For critical events (login, purchases)

### Benefits
- Maximum performance for game ticks
- Reliability for critical events
- Scalable architecture

## Open Questions

1. **Command Validation**: Where should validation happen?
   - API: Basic validation (format, auth)
   - Game Server: Game logic validation
   
2. **Command Results**: Should we store success/failure of commands?

3. **Database Choice**: SQLite vs PostgreSQL vs SQL Server?

4. **Event Sourcing**: Consider for future iterations?

## Next Steps

1. Implement command queue system
2. Add database persistence layer
3. Create command replay mechanism
4. Implement viewport-based updates
5. Add proper player/village management

---

*Last Updated: [Current Date]*