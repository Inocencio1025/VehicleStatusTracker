using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using VehicleTrackerApi.Controllers;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTracker.Tests
{
  public class VehicleControllerTests
  {
    private VehicleController CreateController(VehicleTrackerContext context)
    {
      var loggerMock = new Mock<ILogger<VehicleController>>();

      var clientsMock = new Mock<IHubClients>();
      var clientProxyMock = new Mock<IClientProxy>();

      clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
      clientProxyMock
          .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
          .Returns(Task.CompletedTask);

      var hubMock = new Mock<IHubContext<VehicleHub>>();
      hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

      var options = Options.Create(new DemoTickOptions
      {
        MaxSpeedMph = 120,
        MaxFuelBurn = 2.0,
        EngineCheckChance = 0.1
      });

      return new VehicleController(context, hubMock.Object, loggerMock.Object, options);
    }

    private VehicleTrackerContext CreateContext()
    {
      var options = new DbContextOptionsBuilder<VehicleTrackerContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      return new VehicleTrackerContext(options);
    }

    [Fact]
    public async Task GetVehicleStatus_Empty_ReturnsEmptyList()
    {
      var context = CreateContext();
      var controller = CreateController(context);

      var result = await controller.GetVehicleStatus();

      var ok = Assert.IsType<OkObjectResult>(result);
      var vehicles = Assert.IsType<List<Vehicle>>(ok.Value);

      Assert.Empty(vehicles);
    }

    [Fact]
    public async Task PostVehicleStatus_AddsVehicle()
    {
      var context = CreateContext();
      var controller = CreateController(context);

      var vehicle = new Vehicle
      {
        VehicleId = 1, // ✅ FIXED (was "V1")
        Speed = 60,
        FuelLevel = 90,
        EngineHealth = "Good",
        Timestamp = DateTime.UtcNow,
        Location = new Location { Latitude = 10, Longitude = 10 }
      };

      var result = await controller.PostVehicleStatus(vehicle);

      var ok = Assert.IsType<OkObjectResult>(result);
      var returned = Assert.IsType<Vehicle>(ok.Value);

      Assert.Equal(1, returned.VehicleId); // ✅ FIXED
      Assert.Single(context.Vehicles);
    }

    [Fact]
    public async Task PostVehicleStatus_Null_ReturnsBadRequest()
    {
      var context = CreateContext();
      var controller = CreateController(context);

      var result = await controller.PostVehicleStatus(null!); // suppress warning

      var bad = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal("Vehicle data is required.", bad.Value);
    }
  }
}