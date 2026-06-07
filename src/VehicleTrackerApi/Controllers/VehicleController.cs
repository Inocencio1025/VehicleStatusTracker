using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController(
        VehicleTrackerContext context,
        IHubContext<VehicleHub> hubContext,
        ILogger<VehicleController> logger,
        IOptions<DemoTickOptions> demoOptions) : ControllerBase
    {
        private readonly VehicleTrackerContext _context = context;
        private readonly IHubContext<VehicleHub> _hubContext = hubContext;
        private readonly ILogger<VehicleController> _logger = logger;
        private readonly DemoTickOptions _demoOptions = demoOptions.Value;
        private readonly Random _random = new();

        [Authorize]
        [HttpPost("status")]
        public async Task<IActionResult> CreateVehicleStatus([FromBody] UpdateVehicleStatusRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            if (request == null)
            {
                _logger.LogWarning("Vehicle status payload was null.");
                return BadRequest("Vehicle data is required.");
            }

            Vehicle? vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId);

            if (vehicle == null)
            {
                _logger.LogWarning("Could not find vehicle ID in database");
                return BadRequest("Vehicle does not exist");
            }
            if (userId != vehicle.UserId)
            {
                return Forbid();
            }

            var newStatus = new VehicleStatus
            {
                VehicleId = vehicle.Id,
                Speed = request.Speed,
                FuelLevel = request.FuelLvl,
                EngineHealth = request.EngHlth,
                Timestamp = request.Date,
                Location = request.Location
            };

            _context.VehicleStatuses.Add(newStatus);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("VehicleStatusUpdated", newStatus);

            _logger.LogInformation(
                "Vehicle status create succeeded and broadcast event {EventName} was sent for VehicleId {VehicleId}.",
                "VehicleStatusUpdated",
                request.VehicleId);

            return Ok(newStatus);
        }

        [Authorize]
        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            var existingVehicle = await _context.Vehicles
                .AnyAsync(v => v.VIN == request.VIN);

            if (existingVehicle)
            {
                return BadRequest("Vehicle already exists.");
            }

            var vehicle = new Vehicle(
                request.Make,
                request.Model,
                request.Year,
                request.VIN);

            vehicle.UserId = userId;
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return Created($"/api/vehicle/{vehicle.Id}", vehicle);
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> GetVehicleStatuses()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return Unauthorized();
            }

            var latestStatuses = await _context.VehicleStatuses
                .Where(vs => vs.Vehicle.UserId == userId)
                .Include(vs => vs.Vehicle)
                .OrderByDescending(vs => vs.Timestamp)
                .ToListAsync();

            var result = latestStatuses
                .GroupBy(vs => vs.VehicleId)
                .Select(g => g.First())
                .Select(vs => new VehicleStatusDto(
                    vs.VehicleId,
                    vs.Vehicle.Make,
                    vs.Vehicle.Model,
                    vs.Vehicle.Year,
                    vs.Speed,
                    vs.FuelLevel,
                    vs.EngineHealth,
                    vs.Timestamp,
                    vs.Location
                ))
                .ToList();

            _logger.LogInformation(
                "Returned vehicle status list with {VehicleCount} vehicle(s).",
                result.Count);

            return Ok(result);
        }


    }
}