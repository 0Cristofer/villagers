using Villagers.GameServer;
using Villagers.GameServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services using extension methods
builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddGameServices()
    .AddCorsPolicy(builder.Configuration);

var app = builder.Build();

// Global exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Server error. Please try again later.");
    }
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Note: No UseDeveloperExceptionPage() since we have custom error handling
}

app.UseHttpsRedirection();
app.UseCors("AllowClients");

// No API endpoints needed - using SignalR only

// Configure SignalR for real-time updates to clients
app.MapHub<GameHub>("/gamehub");

app.Run();
