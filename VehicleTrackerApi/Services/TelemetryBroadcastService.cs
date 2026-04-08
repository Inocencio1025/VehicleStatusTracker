using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
    public class TelemetryBroadcastService : BackgroundService
    {
        private const int MaxSpeedMph = 120;
        private readonly IServiceProvider _services;
        private readonly IHubContext<VehicleHub> _hubContext;
        private readonly Random _random = new();

        public TelemetryBroadcastService(IServiceProvider services, IHubContext<VehicleHub> hubContext)
        {
            _services = services;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // BackgroundService runs on app startup and stays alive until shutdown.
            // We keep looping so telemetry keeps updating continuously in development demo
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

                var vehicles = await context.Vehicles.ToListAsync(stoppingToken);
                foreach (var vehicle in vehicles)
                {
                    // Smooth acceleration/braking instead of full random jumps.
                    var speedDelta = _random.Next(-6, 7);
                    vehicle.Speed = Math.Clamp(vehicle.Speed + speedDelta, 0, MaxSpeedMph);

                    // Fuel burn increases at higher speeds, and remains gradual per second.
                    var fuelBurn = 0.01 + (vehicle.Speed / 120.0) * 0.08;
                    vehicle.FuelLevel = Math.Clamp(vehicle.FuelLevel - fuelBurn, 0, 100);

                    // Engine warnings become more likely with low fuel and sustained high speed.
                    var warningChance = 0.01;
                    if (vehicle.FuelLevel < 15) warningChance += 0.10;
                    if (vehicle.Speed > 90) warningChance += 0.05;
                    vehicle.EngineHealth = _random.NextDouble() < warningChance ? "Check Engine" : "Good";

                    vehicle.Timestamp = DateTime.UtcNow;

                    // Move vehicle in small steps rather than teleporting globally.
                    var currentLat = vehicle.Location?.Latitude ?? 42.3314;
                    var currentLng = vehicle.Location?.Longitude ?? -83.0458;
                    var latStep = (_random.NextDouble() - 0.5) * 0.002;
                    var lngStep = (_random.NextDouble() - 0.5) * 0.002;
                    vehicle.Location = new Location
                    {
                        Latitude = Math.Clamp(currentLat + latStep, -90, 90),
                        Longitude = Math.Clamp(currentLng + lngStep, -180, 180)
                    };
                }

                if (vehicles.Count > 0)
                {
                    await context.SaveChangesAsync(stoppingToken);
                    foreach (var vehicle in vehicles)
                    {
                        await _hubContext.Clients.All.SendAsync("VehicleStatusUpdated", vehicle, stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
