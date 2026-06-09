using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Services;

namespace VehicleTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController(VehicleService vehicleService) : ControllerBase
    {

        [Authorize]
        [HttpPost("status")]
        public async Task<IActionResult> CreateVehicleStatus(CreateVehicleStatusRequest request)
        {
            if (GetUserId() is not int userId)
                return Unauthorized();

            var result = await vehicleService.CreateVehicleStatusAsync(
                userId,
                new CreateVehicleStatusInput
                (
                    request.VehicleId,
                    request.Speed,
                    request.Location,
                    request.FuelLvl,
                    request.EngHlth,
                    request.Date
            ));

            if (!result.Success)
            {
                if (result.Message == "Forbidden")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Created($"/api/vehicle/{result.Data!.Id}", result.Data);
        }

        [Authorize]
        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
        {
            if (GetUserId() is not int userId)
                return Unauthorized();


            var result = await vehicleService.CreateVehicleAsync(
                userId,
                new CreateVehicleInput(
                    request.Make,
                    request.Model,
                    request.Year,
                    request.VIN
                ));

            if (!result.Success)
                return BadRequest(result.Message);

            return Created($"/api/vehicle/{result.Data!.Id}", result.Data);
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> GetVehicleStatuses()
        {
            if (GetUserId() is not int userId)
                return Unauthorized();

            var result = await vehicleService.GetStatusesAsync(userId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            if (GetUserId() is not int userId)
                return Unauthorized();
            
            var result = await vehicleService.GetVehiclesAsync(userId);
            
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("vehicles/{id}")]
        public async Task<IActionResult> GetVehicleAsync(int id)
        {
            if (GetUserId() is not int userId)
                return Unauthorized();
            
            var result = await vehicleService.GetVehicleAsync(userId, id);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }


        [Authorize]
        [HttpGet("vehicles/{id}/history")]
        public async Task<IActionResult> GetVehicleHistory(int id, int hours = 24)
        {
            if (GetUserId() is not int userId)
                return Unauthorized();
            
            var result = await vehicleService.GetVehicleHistoryAsync(userId, id);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
                
        [Authorize]
        [HttpDelete("vehicles/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            if (GetUserId() is not int userId)
                return Unauthorized();
            
            var result = await vehicleService.DeleteVehicleAsync(userId, id);

            if (!result.Success)
                return NotFound(result.Message);

            return NoContent();
        }

        //list vehicles per user
        //get single vehicle
        //get single vehicle history (last ? hours)
        //delete vehicle

        private int? GetUserId()
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId)
                ? userId
                : null;
        }
    }
}