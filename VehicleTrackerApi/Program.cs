using Microsoft.EntityFrameworkCore;
using Services;
using VehicleTrackerApi.Data;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// db context
builder.Services.AddDbContext<VehicleTrackerContext>(options =>
    options.UseSqlite("Data Source=vehicles.db"));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllers();          // Enables controller support
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI
builder.Services.AddSwaggerGen();           // Swagger UI generation
builder.Services.AddHostedService<TelemetryBackgroundService>(); //live updates (5 seconds)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();  // Map controller routes

app.Run();
