using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleTrackerContext _context;
        private readonly IHubContext<VehicleHub> _hubContext;
        private readonly ILogger<VehicleController> _logger;
        private readonly Random _random = new();

        public VehicleController(
            VehicleTrackerContext context,
            IHubContext<VehicleHub> hubContext,
            ILogger<VehicleController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetVehicleStatus()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            _logger.LogInformation(
                "Returned vehicle status list with {VehicleCount} vehicle(s).",
                vehicles.Count);
            return Ok(vehicles);
        }

        [HttpPost("status")]
        public async Task<IActionResult> PostVehicleStatus([FromBody] Vehicle vehicle)
        {
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle status payload was null.");
                return BadRequest("Vehicle data is required.");
            }

            _logger.LogDebug(
                "Processing vehicle status update for VehicleId {VehicleId} at {Timestamp}.",
                vehicle.VehicleId,
                vehicle.Timestamp);

            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicle.VehicleId);
            var operation = existingVehicle == null ? "insert" : "update";

            if (existingVehicle == null)
            {
                _context.Vehicles.Add(vehicle);
            }
            else
            {
                existingVehicle.Speed = vehicle.Speed;
                existingVehicle.FuelLevel = vehicle.FuelLevel;
                existingVehicle.EngineHealth = vehicle.EngineHealth;
                existingVehicle.Timestamp = vehicle.Timestamp;
                existingVehicle.Location = vehicle.Location;
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("VehicleStatusUpdated", vehicle);

            _logger.LogInformation(
                "Vehicle status {Operation} succeeded and broadcast event {EventName} was sent for VehicleId {VehicleId}.",
                operation,
                "VehicleStatusUpdated",
                vehicle.VehicleId);

            return Ok(vehicle);
        }

        [HttpPost("demo-tick")]
        public async Task<IActionResult> DemoTick()
        {
            _logger.LogInformation("Demo tick requested.");

            var vehicles = await _context.Vehicles.ToListAsync();
            if (!vehicles.Any())
            {
                _logger.LogWarning("Demo tick skipped because there are no vehicles.");
                return BadRequest("No vehicles found. Add vehicles first.");
            }

            foreach (var vehicle in vehicles)
            {
                vehicle.Speed = _random.Next(0, 121);
                vehicle.FuelLevel = Math.Max(0, vehicle.FuelLevel - _random.NextDouble() * 2);
                vehicle.EngineHealth = _random.Next(0, 10) > 1 ? "Good" : "Check Engine";
                vehicle.Timestamp = DateTime.UtcNow;
                vehicle.Location = new Location
                {
                    Latitude = _random.NextDouble() * 180 - 90,
                    Longitude = _random.NextDouble() * 360 - 180
                };
            }

            await _context.SaveChangesAsync();

            foreach (var updatedVehicle in vehicles)
            {
                await _hubContext.Clients.All.SendAsync("VehicleStatusUpdated", updatedVehicle);
            }

            _logger.LogInformation(
                "Demo tick completed. Updated and broadcast {UpdatedCount} vehicles.",
                vehicles.Count);

            return Ok(new { updated = vehicles.Count });
        }
    }
}
