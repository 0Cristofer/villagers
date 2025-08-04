using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            // Enable JWT authentication for SignalR
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    var hubPath = configuration["Server:HubPath"];
                    
                    // Check if this is a SignalR hub request
                    if (!string.IsNullOrEmpty(accessToken) && 
                        path.StartsWithSegments(hubPath))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

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
        
        // Add player registration service
        services.AddSingleton<IPlayerRegistrationService, PlayerRegistrationService>();
        
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