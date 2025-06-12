using Microsoft.AspNetCore.Mvc;

namespace VehicleTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetVehicleStatus()
        {
            var status = new
            {
                SpeedKph = Random.Shared.Next(0, 120),
                FuelLevel = Random.Shared.NextDouble() * 100,
                EngineHealth = "Good",
                Location = new { Lat = 42.497, Lon = -83.016 },
                Timestamp = DateTime.UtcNow
            };

            return Ok(status);
        }
    }
}
