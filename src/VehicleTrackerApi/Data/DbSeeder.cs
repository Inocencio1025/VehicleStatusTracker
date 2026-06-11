using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Data;

public static class DbSeeder
{
    public static void Seed(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VehicleTrackerContext>();

        // Apply migrations
        if (db.Database.IsRelational())
        {
            db.Database.Migrate();
        }

        // 1. Seed USER first
        if (!db.Users.Any())
        {
            var user = new User
            {
                Username = "demo",
                Email = "demo@test.com",
                Password = "test"
            };

            db.Users.Add(user);
            db.SaveChanges();
        }

        var userId = db.Users.First().Id;

        // 2. Seed VEHICLE
        if (!db.Vehicles.Any())
        {
            var vehicle = new Vehicle("Toyota", "Camry", 2020, "VIN123")
            {
                UserId = userId
            };

            db.Vehicles.Add(vehicle);
            db.SaveChanges();
        }

        var vehicleId = db.Vehicles.First().Id;

        // 3. Seed VEHICLE STATUS
        var vehiclesWithoutStatus = db.Vehicles
            .Where(v => !db.VehicleStatuses.Any(vs => vs.VehicleId == v.Id))
            .ToList();

        if (vehiclesWithoutStatus.Count != 0)
        {
            var random = new Random();

            var statuses = vehiclesWithoutStatus.Select(vehicle => new VehicleStatus
            {
                VehicleId = vehicle.Id,
                Speed = random.Next(0, 120),
                FuelLevel = random.NextDouble() * 100,
                EngineHealth = random.NextDouble() > 0.9 ? "Warning" : "Good",
                Timestamp = DateTime.UtcNow,
                Location = new Location
                {
                    Latitude = random.NextDouble() * 180 - 90,
                    Longitude = random.NextDouble() * 360 - 180
                }
            });

            db.VehicleStatuses.AddRange(statuses);
            db.SaveChanges();
        }
    }
}