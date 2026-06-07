using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Dtos;

namespace VehicleTrackerApi.Services
{
    public class TelemetryBroadcastService(
        IServiceProvider services,
        IHubContext<VehicleHub> hubContext,
        ILogger<TelemetryBroadcastService> logger,
        IOptions<TelemetryOptions> telemetryOptions) : BackgroundService
    {
        private readonly IServiceProvider _services = services;
        private readonly IHubContext<VehicleHub> _hubContext = hubContext;
        private readonly ILogger<TelemetryBroadcastService> _logger = logger;
        private readonly TelemetryOptions _telemetryOptions = telemetryOptions.Value;
        private readonly Random _random = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Telemetry service started (interval: {Interval}s)",
                _telemetryOptions.TickIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

                    // Get latest per vehicle (WITH navigation loaded)
                    var latestStatuses = await context.VehicleStatuses
                        .Include(vs => vs.Vehicle)
                        .GroupBy(vs => vs.VehicleId)
                        .Select(g => g.OrderByDescending(x => x.Timestamp).First())
                        .ToListAsync(stoppingToken);

                    var newStatuses = new List<VehicleStatus>();

                    foreach (var status in latestStatuses)
                    {
                        var newStatus = GenerateNextStatus(status);
                        newStatuses.Add(newStatus);

                        context.VehicleStatuses.Add(newStatus);
                    }

                    await context.SaveChangesAsync(stoppingToken);

                    // reload with Vehicle included (important for DTO mapping)
                    var savedStatuses = await context.VehicleStatuses
                        .Include(vs => vs.Vehicle)
                        .Where(vs => newStatuses.Select(n => n.Id).Contains(vs.Id))
                        .ToListAsync(stoppingToken);

                    foreach (var vs in savedStatuses)
                    {
                        var dto = new VehicleStatusDto(
                            vs.VehicleId,
                            vs.Vehicle.Make,
                            vs.Vehicle.Model,
                            vs.Vehicle.Year,
                            vs.Speed,
                            vs.FuelLevel,
                            vs.EngineHealth,
                            vs.Timestamp,
                            vs.Location
                        );

                        await _hubContext.Clients
                            .User(vs.Vehicle.UserId.ToString())
                            .SendAsync(
                                "VehicleStatusUpdated",
                                dto,
                                stoppingToken);
                    }

                    _logger.LogInformation(
                        "Telemetry tick complete. Updated {Count} vehicles.",
                        newStatuses.Count);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Telemetry tick failed.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(_telemetryOptions.TickIntervalSeconds),
                    stoppingToken);
            }
        }

        private VehicleStatus GenerateNextStatus(VehicleStatus previous)
        {
            var speedDelta = _random.Next(-6, 7);

            var speed = Math.Clamp(
                previous.Speed + speedDelta,
                0,
                _telemetryOptions.MaxSpeedMph);

            var fuelBurn =
                0.01 + (speed / (double)_telemetryOptions.MaxSpeedMph) * 0.08;

            var fuel = Math.Clamp(previous.FuelLevel - fuelBurn, 0, 100);

            var warningChance = 0.01;
            if (fuel < _telemetryOptions.WarningFuelThreshold) warningChance += 0.10;
            if (speed > _telemetryOptions.HighSpeedWarningThreshold) warningChance += 0.05;

            return new VehicleStatus
            {
                VehicleId = previous.VehicleId,
                Speed = speed,
                Location = new Location
                {
                    Latitude = Math.Clamp(
                        (previous.Location?.Latitude ?? 42.3314) + (_random.NextDouble() - 0.5) * 0.002,
                        -90, 90),
                    Longitude = Math.Clamp(
                        (previous.Location?.Longitude ?? -83.0458) + (_random.NextDouble() - 0.5) * 0.002,
                        -180, 180)
                },
                FuelLevel = fuel,
                EngineHealth = _random.NextDouble() < warningChance ? "Check Engine" : "Good",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}