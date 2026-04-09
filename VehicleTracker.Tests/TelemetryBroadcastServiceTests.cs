using Xunit;
using VehicleTrackerApi.Services;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Hubs;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VehicleTracker.Tests
{
  public class TelemetryBroadcastServiceTests
  {
    [Fact]
    public void UpdateVehicles_UpdatesVehicleProperties()
    {
      // Arrange
      var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    VehicleId = 1,
                    Speed = 50,
                    FuelLevel = 80,
                    EngineHealth = "Good",       // Initialize non-nullable property
                    Location = new Location { Latitude = 0, Longitude = 0 }
                }
            };

      var telemetryOptions = Options.Create(new TelemetryOptions
      {
        MaxSpeedMph = 120,
        WarningFuelThreshold = 15,
        HighSpeedWarningThreshold = 90,
        TickIntervalSeconds = 1
      });

      // Mock dependencies (IServiceProvider and SignalR hub)
      var mockServiceProvider = new Mock<IServiceProvider>();
      var mockHub = new Mock<IHubContext<VehicleHub>>();
      var mockLogger = new Mock<ILogger<TelemetryBroadcastService>>();

      // Create service instance
      var service = new TelemetryBroadcastService(
          mockServiceProvider.Object,
          mockHub.Object,
          mockLogger.Object,
          telemetryOptions
      );

      // Act
      service.UpdateVehicles(vehicles);

      var updated = vehicles[0];

      // Assert
      Assert.InRange(updated.Speed, 0, 120);
      Assert.InRange(updated.FuelLevel, 0, 100);
      Assert.NotNull(updated.Location);
      Assert.InRange(updated.Location.Latitude, -90, 90);
      Assert.InRange(updated.Location.Longitude, -180, 180);
      Assert.NotNull(updated.EngineHealth); // Extra safety check
    }
  }
}