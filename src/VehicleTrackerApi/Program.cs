using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi;
using VehicleTrackerApi.Constants;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Extensions;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Services;

var builder = WebApplication.CreateBuilder(args);

//
// ─────────────────────────────────────────────────────────────
//  CORE FRAMEWORK SERVICES
// ─────────────────────────────────────────────────────────────
//

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// Error handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//
// ─────────────────────────────────────────────────────────────
//  AUTHENTICATION / AUTHORIZATION
// ─────────────────────────────────────────────────────────────
//

builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

//
// ─────────────────────────────────────────────────────────────
//  SWAGGER / API DOCUMENTATION
// ─────────────────────────────────────────────────────────────
//

builder.Services.AddAppSwagger();

//
// ─────────────────────────────────────────────────────────────
//  CONFIGURATION (Options Pattern)
// ─────────────────────────────────────────────────────────────
//

builder.Services.Configure<TelemetryOptions>(
    builder.Configuration.GetSection("Telemetry"));

builder.Services.Configure<DemoTickOptions>(
    builder.Configuration.GetSection("DemoTick"));

//
// ─────────────────────────────────────────────────────────────
//  APPLICATION SERVICES (DI)
// ─────────────────────────────────────────────────────────────
//

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VehicleReadService>();
builder.Services.AddScoped<VehicleWriteService>();
builder.Services.AddSingleton<PasswordService>();

//
// ─────────────────────────────────────────────────────────────
//  CORS
// ─────────────────────────────────────────────────────────────
//

builder.Services.AddAppCors(builder.Configuration);

//
// ─────────────────────────────────────────────────────────────
//  DATABASE
// ─────────────────────────────────────────────────────────────
//

var connectionString =
    builder.Configuration.GetConnectionString("VehicleTracker");

if (builder.Environment.IsEnvironment(EnvironmentNames.Testing))
{
    builder.Services.AddDbContext<VehicleTrackerContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<VehicleTrackerContext>(options =>
        options.UseSqlite(connectionString));
}

//
// ─────────────────────────────────────────────────────────────
//  BACKGROUND SERVICES
// ─────────────────────────────────────────────────────────────
//

// Only run telemetry in non-test environments
if (!builder.Environment.IsEnvironment(EnvironmentNames.Testing))
{
    builder.Services.AddHostedService<TelemetryBroadcastService>();
}

var app = builder.Build();

//
// ─────────────────────────────────────────────────────────────
//  DATABASE SEEDING
// ─────────────────────────────────────────────────────────────
//

DbSeeder.Seed(app);

//
// ─────────────────────────────────────────────────────────────
//  MIDDLEWARE PIPELINE
// ─────────────────────────────────────────────────────────────
//

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAppCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<VehicleHub>(Routes.VehicleHub);

//
// ─────────────────────────────────────────────────────────────
//  STARTUP LOGGING
// ─────────────────────────────────────────────────────────────
//

var startupLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup");

startupLogger.LogInformation(
    "VehicleTracker API started in {Environment} environment.",
    app.Environment.EnvironmentName);

app.Run();

public partial class Program { }