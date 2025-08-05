using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Villagers.Api.Configuration;
using Villagers.Api.Data;
using Villagers.Api.Entities;
using Villagers.Api.Services;

namespace Villagers.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApiDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var identitySettings = configuration.GetSection("Identity").Get<IdentitySettings>() ?? new IdentitySettings();

        services.AddIdentity<PlayerEntity, IdentityRole<Guid>>(options =>
        {
            // Password settings from configuration
            options.Password.RequireDigit = identitySettings.Password.RequireDigit;
            options.Password.RequiredLength = identitySettings.Password.RequiredLength;
            options.Password.RequireNonAlphanumeric = identitySettings.Password.RequireNonAlphanumeric;
            options.Password.RequireUppercase = identitySettings.Password.RequireUppercase;
            options.Password.RequireLowercase = identitySettings.Password.RequireLowercase;
            
            // User settings from configuration
            options.User.RequireUniqueEmail = identitySettings.User.RequireUniqueEmail;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        })
        .AddEntityFrameworkStores<ApiDbContext>()
        .AddDefaultTokenProviders();

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
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(corsSettings.AllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IWorldRegistryService, WorldRegistryService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}