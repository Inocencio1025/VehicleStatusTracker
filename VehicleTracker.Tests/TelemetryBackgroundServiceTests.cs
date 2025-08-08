using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Models;

public class TelemetryBackgroundServiceTests
{
    private string _testDbName = Guid.NewGuid().ToString(); 

    private VehicleTrackerContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<VehicleTrackerContext>()
            .UseInMemoryDatabase(databaseName: _testDbName)
            .Options;

        return new VehicleTrackerContext(options);
    }

    [Fact]
    public async Task BackgroundService_UpdatesVehicleData()
    {
        // Arrange 
        using (var context = GetInMemoryContext())
        {
            context.Vehicles.Add(new Vehicle
            {
                VehicleId = 1,
                Speed = 0,
                FuelLevel = 100,
                EngineHealth = "Good",
                Timestamp = DateTime.UtcNow,
                Location = new Location { Latitude = 0, Longitude = 0 }
            });

            await context.SaveChangesAsync();
        }

        var services = new ServiceCollection()
            .AddScoped(_ => GetInMemoryContext())
            .BuildServiceProvider();

        var service = new TelemetryBackgroundService(services);

        // Act
        var cancelToken = new CancellationTokenSource();
        var task = service.StartAsync(cancelToken.Token);
        await Task.Delay(6000); // Wait > 5 seconds
        cancelToken.Cancel();
        await task;

        using var verifyContext = GetInMemoryContext();
        var updatedVehicle = await verifyContext.Vehicles.FirstAsync();

        Assert.InRange(updatedVehicle.Speed, 0, 120);
        Assert.InRange(updatedVehicle.FuelLevel, 0, 100);
        Assert.NotNull(updatedVehicle.EngineHealth);
        Assert.True(updatedVehicle.Timestamp > DateTime.UtcNow.AddMinutes(-1));
    }
}
