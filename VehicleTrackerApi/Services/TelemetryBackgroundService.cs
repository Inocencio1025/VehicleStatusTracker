using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Services
{
  public class TelemetryBackgroundService : BackgroundService
  {
    private readonly IServiceProvider _services;
    private readonly Random _random = new();

    public TelemetryBackgroundService(IServiceProvider services)
    {
      _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        using (var scope = _services.CreateScope())
        {
          var context = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

          var vehicles = await context.Vehicles.ToListAsync(stoppingToken);

          foreach (var vehicle in vehicles)
          {
            vehicle.Speed = _random.Next(0, 121);
            vehicle.FuelLevel = Math.Max(0, vehicle.FuelLevel - _random.NextDouble() * 0.5);
            vehicle.EngineHealth = _random.Next(0, 10) > 1 ? "Good" : "Check Engine";
            vehicle.Timestamp = DateTime.UtcNow;
            vehicle.Location = new Location
            {
              Latitude = _random.NextDouble() * 180 - 90,
              Longitude = _random.NextDouble() * 360 - 180
            };
          }

          await context.SaveChangesAsync(stoppingToken);
        }

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
      }
    }
  }

}
