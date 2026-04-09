using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
    public class TelemetryBroadcastService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IHubContext<VehicleHub> _hubContext;
        private readonly ILogger<TelemetryBroadcastService> _logger;
        private readonly TelemetryOptions _telemetryOptions;
        private readonly Random _random = new();
        private int _tickCounter;

        public TelemetryBroadcastService(
            IServiceProvider services,
            IHubContext<VehicleHub> hubContext,
            ILogger<TelemetryBroadcastService> logger,
            IOptions<TelemetryOptions> telemetryOptions)
        {
            _services = services;
            _hubContext = hubContext;
            _logger = logger;
            _telemetryOptions = telemetryOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Telemetry broadcast service started with interval {IntervalSeconds}s.",
                _telemetryOptions.TickIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

                    var vehicles = await context.Vehicles.ToListAsync(stoppingToken);
                    foreach (var vehicle in vehicles)
                    {
                        var speedDelta = _random.Next(-6, 7);
                        vehicle.Speed = Math.Clamp(vehicle.Speed + speedDelta, 0, _telemetryOptions.MaxSpeedMph);

                        var fuelBurn = 0.01 + (vehicle.Speed / (double)_telemetryOptions.MaxSpeedMph) * 0.08;
                        vehicle.FuelLevel = Math.Clamp(vehicle.FuelLevel - fuelBurn, 0, 100);

                        var warningChance = 0.01;
                        if (vehicle.FuelLevel < _telemetryOptions.WarningFuelThreshold) warningChance += 0.10;
                        if (vehicle.Speed > _telemetryOptions.HighSpeedWarningThreshold) warningChance += 0.05;
                        vehicle.EngineHealth = _random.NextDouble() < warningChance ? "Check Engine" : "Good";

                        vehicle.Timestamp = DateTime.UtcNow;

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

                    _tickCounter++;
                    if (vehicles.Count > 0)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        foreach (var vehicle in vehicles)
                        {
                            await _hubContext.Clients.All.SendAsync("VehicleStatusUpdated", vehicle, stoppingToken);
                        }

                        if (_tickCounter % 10 == 0)
                        {
                            _logger.LogInformation(
                                "Telemetry tick {Tick}: updated and broadcast {VehicleCount} vehicles.",
                                _tickCounter,
                                vehicles.Count);
                        }
                    }
                    else if (_tickCounter % 10 == 0)
                    {
                        _logger.LogInformation(
                            "Telemetry tick {Tick}: no vehicles found to update.",
                            _tickCounter);
                    }
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Telemetry tick failed.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_telemetryOptions.TickIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Telemetry broadcast service stopped.");
        }
    }
}