using Microsoft.EntityFrameworkCore;
using Villagers.GameServer.Configuration;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Repositories;
using Villagers.GameServer.Services;

namespace Villagers.GameServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GameDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorldRepository, WorldRepository>();
        services.AddScoped<ICommandRepository, CommandRepository>();
        
        return services;
    }

    public static IServiceCollection AddGameServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register and validate world configuration
        services.Configure<WorldConfiguration>(configuration);
        services.AddOptions<WorldConfiguration>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        // Add HTTP client for API communication
        services.AddHttpClient();
        
        // Add world registration service
        services.AddSingleton<IWorldRegistrationService, WorldRegistrationService>();
        
        services.AddSignalR();
        services.AddSingleton<IGameSimulationService, GameSimulationService>();
        services.AddHostedService(provider => 
            provider.GetRequiredService<IGameSimulationService>());
        
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["https://localhost:3000"];
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowClients", policy =>
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}