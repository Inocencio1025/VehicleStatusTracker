using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi;
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
builder.Services.AddControllers();          // Enables controller support
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI
builder.Services.AddSwaggerGen();           // Swagger UI generation
builder.Services.AddSignalR();              // SignalR for real-time communication
builder.Services.AddHostedService<TelemetryBroadcastService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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
builder.Services.AddDbContext<VehicleTrackerContext>(options =>
    options.UseSqlite(connectionString));

// Configure options
builder.Services.Configure<TelemetryOptions>(builder.Configuration.GetSection("Telemetry"));
builder.Services.Configure<DemoTickOptions>(builder.Configuration.GetSection("DemoTick"));

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();  // Map controller routes
app.MapHub<VehicleHub>("/hubs/vehicle");

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

startupLogger.LogInformation(
    "VehicleTracker API started in {Environment} environment.",
    app.Environment.EnvironmentName);

app.Run();
