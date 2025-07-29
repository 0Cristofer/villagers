using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Domain.Commands;
using Villagers.GameServer.Infrastructure.Repositories;
using Villagers.Shared.Entities;

namespace Villagers.GameServer.Services;

public class PersistenceService : IPersistenceService
{
    private readonly IWorldStateRepository _worldStateRepository;
    private readonly ICommandRepository _commandRepository;
    private readonly ILogger<PersistenceService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public PersistenceService(
        IWorldStateRepository worldStateRepository,
        ICommandRepository commandRepository,
        ILogger<PersistenceService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _worldStateRepository = worldStateRepository;
        _commandRepository = commandRepository;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task SaveWorldStateAsync(long currentTick, bool isRunning = true)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var worldStateRepo = scope.ServiceProvider.GetRequiredService<IWorldStateRepository>();
            
            await worldStateRepo.UpdateStateAsync(currentTick, isRunning);
            _logger.LogDebug("World state saved for tick {CurrentTick}", currentTick);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save world state for tick {CurrentTick}", currentTick);
        }
    }

    public async Task ProcessCommandsAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var commandRepo = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
            
            var pendingCommands = await commandRepo.GetCommandsBatchAsync(50);
            
            foreach (var command in pendingCommands)
            {
                try
                {
                    // Mark command as processing
                    await commandRepo.UpdateStatusAsync(command.Id, CommandStatus.Processing);
                    
                    // Here we would convert the persisted command back to domain command
                    // and process it through the game simulation
                    // For now, we'll just mark it as completed
                    await commandRepo.UpdateStatusAsync(command.Id, CommandStatus.Completed);
                    
                    _logger.LogDebug("Processed command {CommandId} of type {CommandType}", 
                        command.Id, command.Type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process command {CommandId}", command.Id);
                    await commandRepo.UpdateStatusAsync(command.Id, CommandStatus.Failed, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process command batch");
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var worldStateRepo = scope.ServiceProvider.GetRequiredService<IWorldStateRepository>();
            
            var worldState = await worldStateRepo.GetCurrentStateAsync();
            _logger.LogInformation("Persistence service initialized. Current tick: {CurrentTick}, Running: {IsRunning}", 
                worldState.CurrentTick, worldState.IsRunning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize persistence service");
            throw;
        }
    }
}