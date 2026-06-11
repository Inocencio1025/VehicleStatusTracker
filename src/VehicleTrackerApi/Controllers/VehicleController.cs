using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Extensions;
using VehicleTrackerApi.Services;

namespace VehicleTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController(
        VehicleReadService vehicleReadService,
        VehicleWriteService vehicleWriteService) : ControllerBase
    {

        [Authorize]
        [HttpPost("status")]
        public async Task<IActionResult> CreateVehicleStatus(CreateVehicleStatusRequest request)
        {
            var result = await vehicleWriteService.CreateVehicleStatusAsync(
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
                return NotFound(result.Message);

            return Created($"/api/vehicle/{result.Data!.VehicleId}", result.Data.VehicleId);
        }

        [Authorize]
        [HttpPost("vehicle")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await vehicleWriteService.CreateVehicleAsync(
                userId.Value,
                new CreateVehicleInput(
                    request.Make,
                    request.Model,
                    request.Year,
                    request.VIN
                ));

            if (!result.Success)
                return BadRequest(result.Message);

            return Created($"/api/vehicle/{result.Data!.VehicleId}", result.Data.VehicleId);
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> GetVehicleStatuses()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            var result = await vehicleReadService.GetLatestStatusesAsync(userId.Value);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();
            
            var result = await vehicleReadService.GetVehiclesByUserAsync(userId.Value);
            
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();
            
            var result = await vehicleReadService.GetVehicleAsync(userId.Value, id);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }


        [Authorize]
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetVehicleHistory(int id, int hours = 24)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();
            
            var result = await vehicleReadService.GetVehicleHistoryAsync(userId.Value, id, hours);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
                
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();
            
            var result = await vehicleWriteService.DeleteVehicleAsync(userId.Value, id);

            if (!result.Success)
                return NotFound(result.Message);

            return NoContent();
        }

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