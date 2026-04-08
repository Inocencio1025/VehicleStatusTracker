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
        private readonly Random _random = new();

        public VehicleController(VehicleTrackerContext context, IHubContext<VehicleHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetVehicleStatus()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return Ok(vehicles);
        }

        [HttpPost("status")]
        public async Task<IActionResult> PostVehicleStatus([FromBody] Vehicle vehicle)
        {
            if (vehicle == null)
                return BadRequest("Vehicle data is required.");

            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicle.VehicleId);

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

            return Ok(vehicle);
        }

        [HttpPost("demo-tick")]
        public async Task<IActionResult> DemoTick()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            if (!vehicles.Any())
                return BadRequest("No vehicles found. Add vehicles first.");

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

            return Ok(new { updated = vehicles.Count });
        }
    }
}
