using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;

namespace VehicleTrackerApi.Services
{
  public class VehicleReadService(VehicleTrackerContext context)
  {
    public async Task<Result<List<VehicleStatusDto>>> GetLatestStatusesAsync(int userId)
    {
      var latestStatuses = await context.VehicleStatuses
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

      return new Result<List<VehicleStatusDto>>(true, "Success", result);
    }

    public async Task<Result<VehicleHistoryDto>> GetVehicleHistoryAsync(int userId, int id, int hours)
    {
      var existingVehicle = await context.Vehicles
        .FirstOrDefaultAsync(v => v.UserId == userId && v.Id == id);

      if (existingVehicle == null)
        return new Result<VehicleHistoryDto>(false, "Vehicle does not exist", null);

      var cutoff = DateTime.UtcNow.AddHours(-hours);

      var statusHistory = await context.VehicleStatuses
        .Where(vs => vs.VehicleId == id &&
          vs.Vehicle.UserId == userId &&
          vs.Timestamp >= cutoff)
        .Include(vs => vs.Vehicle)
        .OrderByDescending(vs => vs.Timestamp)
        .ToListAsync();

        var avgSpeed = (int)statusHistory.Average(vs => vs.Speed);
        var maxSpeed = statusHistory.Max(vs => vs.Speed);
        var totalMileage = (int)statusHistory.Sum(vs => vs.Speed * (vs.Timestamp - cutoff).TotalHours);
        
        DateTime? lastRefueled = null;
        for (int i = 1; i < statusHistory.Count; i++)
        {
            var current = statusHistory[i - 1];
            var previous = statusHistory[i];

            if (current.FuelLevel - previous.FuelLevel > 10) // threshold
            {
                lastRefueled = current.Timestamp;
                break;
            }
        }

        var historyList = statusHistory
          .Select(vs => new VehicleStatusDto(
            vs.VehicleId,
            vs.Vehicle.Make,
            vs.Vehicle.Model,
            vs.Vehicle.Year,
            vs.Speed,
            vs.FuelLevel,
            vs.EngineHealth,
            vs.Timestamp,
            vs.Location))
          .ToList();

      return new Result<VehicleHistoryDto>(true, "Success", 
        new VehicleHistoryDto(
          avgSpeed,
          maxSpeed,
          totalMileage,
          lastRefueled,
          historyList
        )
      );
    }

    public async Task<Result<List<VehicleDto>>> GetVehiclesByUserAsync(int userId)
    {
      var vehicles = await context.Vehicles
        .Where(v => v.UserId == userId)
        .Select(v => new VehicleDto(
          v.Id,
          v.Make,
          v.Model,
          v.Year,
          v.VIN
        ))
        .ToListAsync();

      return new Result<List<VehicleDto>>(true, "Success", vehicles);
    }

    public async Task<Result<VehicleDto>> GetVehicleAsync(int userId, int vehicleId)
    {
      var vehicle = await context.Vehicles
        .FirstOrDefaultAsync(v => v.Id == vehicleId && v.UserId == userId);

      if (vehicle == null)
        return new Result<VehicleDto>(false, "Vehicle does not exist", null);

      var vehicleDto = new VehicleDto(
        vehicle.Id,
        vehicle.Make,
        vehicle.Model,
        vehicle.Year,
        vehicle.VIN
      );

      return new Result<VehicleDto>(true, "Success", vehicleDto);
    }

  }
}