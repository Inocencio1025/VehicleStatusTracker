using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Services;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleTrackerApi.Controllers;

namespace VehicleTracker.Tests
{
  public class VehicleServiceTests
  {
    private static VehicleTrackerContext CreateContext()
    {
      var options = new DbContextOptionsBuilder<VehicleTrackerContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      return new VehicleTrackerContext(options);
    }

    [Fact]
    public async Task GetVehicleAsync_ReturnsVehicle_WhenVehicleExistsAndBelongsToUser()
    {
      // Arrange
      var context = CreateContext();

      var vehicle = new Vehicle("Toyota", "Camry", 2020, "VIN123")
      {
        Id = 1,
        UserId = 10
      };

      context.Vehicles.Add(vehicle);
      await context.SaveChangesAsync();

      var service = new VehicleService(
          context,
          null!,   // hubContext not needed for this test
          null!    // logger not needed for this test
      );

      // Act
      var result = await service.GetVehicleAsync(10, 1);

      // Assert
      Assert.True(result.Success);
      Assert.NotNull(result.Data);
      Assert.Equal("Toyota", result.Data.Make);
      Assert.Equal("Camry", result.Data.Model);
    }

    [Fact]
    public async Task GetVehicleAsync_WhenVehicleDoesNotExist_ReturnsFailure()
    {
      // Arrange
      var context = CreateContext();

      var service = new VehicleService(
          context,
          null!,
          null!
      );

      // Act
      var result = await service.GetVehicleAsync(userId: 10, vehicleId: 1);

      // Assert
      Assert.False(result.Success);
      Assert.Null(result.Data);
      Assert.Equal("Vehicle does not exist", result.Message);
    }
    [Fact]
    public async Task CreateVehicleAsync_ValidInput_ReturnsSuccess()
    {
      // Arrange
      var context = CreateContext();

      var service = new VehicleService(
          context,
          null!,
          NullLogger<VehicleService>.Instance
      );

      var input = new CreateVehicleInput(
          "Toyota",
          "Camry",
          2020,
          "VIN123"
      );

      // Act
      var result = await service.CreateVehicleAsync(userId: 1, input);

      // Assert
      Assert.True(result.Success);
      Assert.NotNull(result.Data);
      Assert.Equal("Toyota", result.Data.Make);
      Assert.Equal(1, result.Data.UserId);
    }
    [Fact]
    public async Task CreateVehicleAsync_WhenVinAlreadyExists_ReturnsFailure()
    {
      // Arrange
      var context = CreateContext();

      context.Vehicles.Add(new Vehicle("Toyota", "Camry", 2020, "VIN123")
      {
        UserId = 1
      });

      await context.SaveChangesAsync();

      var service = new VehicleService(
          context,
          null!,
          NullLogger<VehicleService>.Instance
      );

      var input = new CreateVehicleInput(
          "Honda",
          "Civic",
          2022,
          "VIN123"
      );

      // Act
      var result = await service.CreateVehicleAsync(1, input);

      // Assert
      Assert.False(result.Success);
      Assert.Equal("Vehicle already exists.", result.Message);
      Assert.Null(result.Data);
    }
  }
}