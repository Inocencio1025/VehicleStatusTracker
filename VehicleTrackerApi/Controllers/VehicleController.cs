using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleTrackerContext _context;

        public VehicleController(VehicleTrackerContext context)
        {
            _context = context;
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

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return Ok(vehicle);
        }
    }
}
