using Villagers.GameServer;
using Villagers.GameServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add world configuration from separate files
builder.Configuration.AddJsonFile("worldconfig.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"worldconfig.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services using extension methods
builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddJwtAuthentication(builder.Configuration)
    .AddGameServices(builder.Configuration)
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

app.UseAuthentication();
app.UseAuthorization();

// No API endpoints needed - using SignalR only

// Configure SignalR for real-time updates to clients
var hubPath = builder.Configuration["Server:HubPath"] ?? "/gamehub";
app.MapHub<GameHub>(hubPath);

app.Run();
