# Database Persistence Implementation for Villagers Game

## Conversation Summary

### Initial Discussion: Persistence Strategy

**Question**: How should game state persistence work?
- Game state persisted at end of tick after a certain period
- Commands persisted by API when acknowledged
- Each domain roughly maps to different tables (e.g., villages table)

**Key Architecture Decision**: Hybrid persistence approach
- **Reference Data** (players, user accounts): API can update directly
- **Game State** (villages, resources, troops): Only through game loop
- **Commands**: Always persisted by API first, then processed by game server

### Database Provider Selection

**Choice: PostgreSQL**
- ACID compliance for game state consistency
- Excellent JSON support for complex game objects
- First-class .NET/EF Core support
- Good scalability with read replicas
- Free and mature

### Implementation Steps

#### 1. Package Installation
```bash
# Added to both projects:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design  
- Npgsql.EntityFrameworkCore.PostgreSQL
```

#### 2. Shared Data Models Library
Created `/shared/Villagers.Shared/` with entities:

**Player.cs**
- Id, Username, Email
- CreatedAt, UpdatedAt
- Unique constraints on username and email

**WorldState.cs**
- Single row for world state
- CurrentTick, LastUpdated, IsRunning

**Command.cs**
- Id, Type, Payload (JSON), PlayerId
- Status enum: Pending, Processing, Completed, Failed
- CreatedAt, ProcessedAt, ErrorMessage

#### 3. Infrastructure Layer

**API Infrastructure**:
- `ApiDbContext`: Manages Players and Commands tables
- `IPlayerRepository`/`PlayerRepository`: CRUD operations for players
- `ICommandRepository`/`CommandRepository`: Command queue operations

**Game Server Infrastructure**:
- `GameDbContext`: Manages WorldState and Commands tables
- `IWorldStateRepository`/`WorldStateRepository`: World state persistence
- `ICommandRepository`/`CommandRepository`: Process pending commands

#### 4. Database Configuration

**Connection Strings**:
- API: `Host=localhost;Database=villagers_api;Username=villagers_user;Password=villagers_password`
- Game Server: `Host=localhost;Database=villagers_game;Username=villagers_user;Password=villagers_password`

**Dependency Injection Setup**:
```csharp
// Both projects
builder.Services.AddDbContext<DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IRepository, Repository>();
```

#### 5. Background Persistence Service

Created `PersistenceBackgroundService`:
- Saves world state every 30 seconds
- Processes commands every 5 seconds
- Uses `IServiceScopeFactory` to handle scoped dependencies in singleton service

#### 6. API Updates

**New PlayerController**:
- GET /api/player/{id}
- GET /api/player/username/{username}
- POST /api/player
- HEAD /api/player/username/{username}
- HEAD /api/player/email/{email}

**Updated CommandController**:
- Persists commands to database before sending to game server
- Updates command status based on game server response
- Returns command ID for tracking

#### 7. Database Migrations

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --context ApiDbContext
dotnet ef migrations add InitialCreate --context GameDbContext
```

Created migrations with:
- Proper PostgreSQL types (uuid, jsonb, timestamp with time zone)
- Indexes on status and created_at for commands
- Unique constraints on username and email

#### 8. Test Updates

- Updated all controller tests to mock repository dependencies
- Fixed dependency injection scope issues
- All 36 unit tests passing
- Integration tests excluded (require actual database)

### Key Design Decisions

1. **Separation of Concerns**:
   - API owns identity/authentication data
   - Game Server owns simulation state
   - Commands are shared but processed by Game Server

2. **Clean Architecture**:
   - Domain layer has no database dependencies
   - Infrastructure layer handles all persistence
   - Repository pattern for data access abstraction

3. **Resilience Patterns**:
   - Commands persisted immediately (never lost)
   - Background service handles failures gracefully
   - Periodic state snapshots for recovery

4. **Performance Considerations**:
   - Batch command processing (50 at a time)
   - Separate intervals for different operations
   - PostgreSQL jsonb for efficient complex data storage

### Architecture Benefits

✅ Fast user registration/login (no game tick wait)
✅ Commands never lost due to immediate persistence
✅ Game simulation remains pure and testable
✅ Clear boundaries between services
✅ Scalable with read replicas for API queries
✅ SignalR still handles real-time updates

### Next Steps (Not Implemented)

1. Add more domain entities as game features grow
2. Implement actual command-to-game-state integration
3. Add database connection pooling for production
4. Set up test databases for integration tests
5. Add data migration scripts for production deployment

### Files Created/Modified

**New Files**:
- `/shared/Villagers.Shared/` - Entire shared library
- `/api/Infrastructure/` - Data layer for API
- `/game-server/Infrastructure/` - Data layer for Game Server
- `/api/src/Controllers/PlayerController.cs`
- `/game-server/Services/PersistenceService.cs`
- `/game-server/Services/PersistenceBackgroundService.cs`
- `/api.tests/Controllers/PlayerControllerTests.cs`

**Modified Files**:
- Both `Program.cs` files - Added EF Core configuration
- Both `appsettings.json` files - Added connection strings
- `/api/src/Controllers/CommandController.cs` - Added persistence
- All existing test files - Added repository mocks

### Testing Results

- API Unit Tests: 16/16 passed ✅
- Game Server Unit Tests: 20/20 passed ✅
- Integration Tests: Failing (expected - no test database)
- Build: 0 warnings, 0 errors ✅

### Summary

Successfully implemented a robust database persistence layer that:
- Maintains clean separation between game logic and data storage
- Provides immediate persistence for critical data
- Supports the existing microservices architecture
- Enables future scaling and feature development

The implementation follows best practices for ASP.NET Core applications with Entity Framework Core and PostgreSQL, providing a solid foundation for the Villagers game's data persistence needs.