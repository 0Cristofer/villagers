using Villagers.GameServer;
using Villagers.GameServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSignalR();
builder.Services.AddSingleton<IGameSimulationService, GameSimulationService>();
builder.Services.AddHostedService(provider => 
    provider.GetRequiredService<IGameSimulationService>());
builder.Services.AddControllers();

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

// Configure API endpoints
app.MapControllers();

// Configure SignalR for real-time updates to clients
app.MapHub<GameHub>("/gamehub");

app.Run();
