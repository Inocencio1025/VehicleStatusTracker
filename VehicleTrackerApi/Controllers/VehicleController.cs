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
            // add sample cars to list
            var vehicles = new List<Vehicle>();
            var rand = new Random();

            for (int i = 0; i < 5; i++)
            {
                var vehicle = new Vehicle
                {
                    VehicleId = i + 1,
                    Speed = rand.Next(0, 120),
                    FuelLevel = rand.NextDouble() * 100,
                    EngineHealth = rand.Next(0, 2) == 0 ? "Good" : "Needs Maintenance",
                    Timestamp = DateTime.UtcNow,
                    Location = new Location
                    {
                        Latitude = rand.NextDouble() * 180 - 90,
                        Longitude = rand.NextDouble() * 360 - 180
                    }
                };
                vehicles.Add(vehicle);
            }
            return Ok(vehicles);
        }
        [HttpPost("status")]
        public IActionResult PostVehicleStatus([FromBody] Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return BadRequest("Vehicle data is required.");
            }

            // You could add logging or persistence here in the future.
            return Ok(vehicle); // Echo back the received data for now.
        }
    }
}
