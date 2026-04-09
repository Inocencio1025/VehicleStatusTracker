using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleTrackerApi.Controllers;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;
using Xunit;

namespace VehicleTracker.Tests.Controllers
{
    public class VehicleControllerTests
    {
        private sealed class TestClientProxy : IClientProxy
        {
            public string? LastMethodName { get; private set; }
            public object?[] LastArgs { get; private set; } = [];

            public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
            {
                LastMethodName = method;
                LastArgs = args;
                return Task.CompletedTask;
            }
        }

        private sealed class TestHubClients : IHubClients
        {
            private readonly IClientProxy _allClientProxy;

            public TestHubClients(IClientProxy allClientProxy)
            {
                _allClientProxy = allClientProxy;
            }

            public IClientProxy All => _allClientProxy;
            public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => _allClientProxy;
            public IClientProxy Client(string connectionId) => _allClientProxy;
            public IClientProxy Clients(IReadOnlyList<string> connectionIds) => _allClientProxy;
            public IClientProxy Group(string groupName) => _allClientProxy;
            public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => _allClientProxy;
            public IClientProxy Groups(IReadOnlyList<string> groupNames) => _allClientProxy;
            public IClientProxy User(string userId) => _allClientProxy;
            public IClientProxy Users(IReadOnlyList<string> userIds) => _allClientProxy;
        }

        private sealed class TestGroupManager : IGroupManager
        {
            public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class TestHubContext : IHubContext<VehicleHub>
        {
            public TestHubContext(IHubClients clients)
            {
                Clients = clients;
            }

            public IHubClients Clients { get; }
            public IGroupManager Groups { get; } = new TestGroupManager();
        }

        private VehicleTrackerContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<VehicleTrackerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VehicleTrackerContext(options);
        }

        [Fact]
        public async Task GetVehicleStatus_ReturnsAllVehicles()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Vehicles.Add(new Vehicle { Speed = 60, FuelLevel = 50.5, EngineHealth = "Good", Timestamp = DateTime.UtcNow });
            context.Vehicles.Add(new Vehicle { Speed = 30, FuelLevel = 80.0, EngineHealth = "Fair", Timestamp = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var hubContext = new TestHubContext(new TestHubClients(new TestClientProxy()));
            var controller = new VehicleController(context, hubContext, NullLogger<VehicleController>.Instance);

            // Act
            var result = await controller.GetVehicleStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var vehicles = Assert.IsAssignableFrom<IEnumerable<Vehicle>>(okResult.Value);
            Assert.Equal(2, ((List<Vehicle>)vehicles).Count);
        }

        [Fact]
        public async Task PostVehicleStatus_AddsVehicle_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var clientProxy = new TestClientProxy();
            var hubContext = new TestHubContext(new TestHubClients(clientProxy));
            var controller = new VehicleController(context, hubContext, NullLogger<VehicleController>.Instance);

            var vehicle = new Vehicle
            {
                Speed = 70,
                FuelLevel = 40.0,
                EngineHealth = "Excellent",
                Timestamp = DateTime.UtcNow,
                Location = new Location { Latitude = 42.5, Longitude = -83.1 }
            };

            // Act
            var result = await controller.PostVehicleStatus(vehicle);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedVehicle = Assert.IsType<Vehicle>(okResult.Value);

            Assert.Equal(vehicle.Speed, returnedVehicle.Speed);
            Assert.Equal(1, await context.Vehicles.CountAsync());
            Assert.Equal("VehicleStatusUpdated", clientProxy.LastMethodName);
            Assert.Single(clientProxy.LastArgs);
        }

        [Fact]
        public async Task PostVehicleStatus_ReturnsBadRequest_WhenNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var hubContext = new TestHubContext(new TestHubClients(new TestClientProxy()));
            var controller = new VehicleController(context, hubContext, NullLogger<VehicleController>.Instance);

            // Act
            var result = await controller.PostVehicleStatus(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Vehicle data is required.", badRequest.Value);
        }
    }
}
