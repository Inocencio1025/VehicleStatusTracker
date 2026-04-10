using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Services;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("VehicleTracker");

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Only run background service outside of tests
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<TelemetryBroadcastService>();
}

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// DB context
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<VehicleTrackerContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<VehicleTrackerContext>(options =>
        options.UseSqlite(connectionString));
}

// Configure options
builder.Services.Configure<TelemetryOptions>(builder.Configuration.GetSection("Telemetry"));
builder.Services.Configure<DemoTickOptions>(builder.Configuration.GetSection("DemoTick"));

var app = builder.Build();


// =========================
// DATABASE MIGRATION + SEED
// =========================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

    db.Database.Migrate();

    if (!db.Vehicles.Any())
    {
        var random = new Random();

        var vehicles = new List<Vehicle>();

        for (int i = 0; i < 12; i++)
        {
            vehicles.Add(new Vehicle
            {
                Speed = random.Next(0, 120),
                FuelLevel = random.NextDouble() * 100,
                EngineHealth = random.NextDouble() > 0.9 ? "Warning" : "Good",
                Timestamp = DateTime.UtcNow,
                Location = new Location
                {
                    Latitude = random.NextDouble() * 180 - 90,
                    Longitude = random.NextDouble() * 360 - 180
                }
            });
        }

        db.Vehicles.AddRange(vehicles);
        db.SaveChanges();
    }
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();
app.MapHub<VehicleHub>("/hubs/vehicle");

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup");

startupLogger.LogInformation(
    "VehicleTracker API started in {Environment} environment.",
    app.Environment.EnvironmentName);

app.Run();

public partial class Program { }