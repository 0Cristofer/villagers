using Microsoft.EntityFrameworkCore;
using Villagers.GameServer;
using Villagers.GameServer.Infrastructure.Data;
using Villagers.GameServer.Infrastructure.Repositories;
using Villagers.GameServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add database
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories (keep for future command persistence)
builder.Services.AddScoped<IWorldRepository, WorldRepository>();
builder.Services.AddScoped<ICommandRepository, CommandRepository>();

// Add services to the container
builder.Services.AddSignalR();
builder.Services.AddSingleton<IGameSimulationService, GameSimulationService>();
builder.Services.AddHostedService(provider => 
    provider.GetRequiredService<IGameSimulationService>());

// Add CORS for client connections
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001") // React app + API Gateway
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowClients");

// No API endpoints needed - using SignalR only

// Configure SignalR for real-time updates to clients
app.MapHub<GameHub>("/gamehub");

app.Run();
