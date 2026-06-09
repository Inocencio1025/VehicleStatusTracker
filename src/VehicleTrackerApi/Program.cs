using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VehicleTrackerApi;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Services;

// CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Load configuration (from appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("VehicleTracker");
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

var jwtKey = builder.Configuration["Jwt:Key"];

// Add services to the container (DI)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/hubs/vehicle"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// Configuration for options pattern
builder.Services.Configure<TelemetryOptions>(builder.Configuration.GetSection("Telemetry"));
builder.Services.Configure<DemoTickOptions>(builder.Configuration.GetSection("DemoTick"));

// Prevents telemetry broadcasting during tests
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<TelemetryBroadcastService>();
}

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<VehicleService>();

// CORS
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

// Database configuration
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

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }

    // 1. Seed USER first
    if (!db.Users.Any())
    {
        var user = new User
        {
            Username = "demo",
            Email = "demo@test.com",
            Password = "test" // doesn't matter for seed
        };

        db.Users.Add(user);
        db.SaveChanges();
    }

    var userId = db.Users.First().Id;

    // 2. Seed VEHICLE
    if (!db.Vehicles.Any())
    {
        var vehicle = new Vehicle("Toyota", "Camry", 2020, "VIN123")
        {
            UserId = userId
        };

        db.Vehicles.Add(vehicle);
        db.SaveChanges();
    }

    var vehicleId = db.Vehicles.First().Id;

    // 3. Seed VEHICLE STATUS
    var vehiclesWithoutStatus = db.Vehicles
    .Where(v => !db.VehicleStatuses.Any(vs => vs.VehicleId == v.Id))
    .ToList();

    if (vehiclesWithoutStatus.Count != 0)
    {
        var random = new Random();

        var statuses = vehiclesWithoutStatus.Select(vehicle => new VehicleStatus
        {
            VehicleId = vehicle.Id,
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

        db.VehicleStatuses.AddRange(statuses);
        db.SaveChanges();
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<VehicleHub>("/hubs/vehicle");

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup");

startupLogger.LogInformation(
    "VehicleTracker API started in {Environment} environment.",
    app.Environment.EnvironmentName);

app.Run();

public partial class Program { }