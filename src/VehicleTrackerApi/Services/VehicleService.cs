using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Controllers;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
  public class VehicleService(
    VehicleTrackerContext context,
    IHubContext<VehicleHub> hubContext,
    ILogger<VehicleController> logger)
  {
    public async Task<Result<Vehicle>> CreateVehicleAsync(
      int userId,
      CreateVehicleInput request)
    {

      var existingVehicle = await context.Vehicles
        .AnyAsync(v => v.VIN == request.VIN);

      if (existingVehicle)
        return new Result<Vehicle>(false, "Vehicle already exists.", null);

      var newVehicle = new Vehicle(
          request.Make,
          request.Model,
          request.Year,
          request.VIN)
      {
        UserId = userId
      };

      context.Vehicles.Add(newVehicle);
      await context.SaveChangesAsync();

      logger.LogInformation(
        "Vehicle Created with ID: {VehicleId}",
        newVehicle.Id);

      return new Result<Vehicle>(true, "Created", newVehicle);
    }

    public async Task<Result<VehicleStatus>> CreateVehicleStatusAsync(
      int userId,
      CreateVehicleStatusInput request)
    {
      var vehicle = await context.Vehicles
        .FirstOrDefaultAsync(v => v.Id == request.VehicleId);

      if (vehicle == null)
        return new Result<VehicleStatus>(false, "Vehicle not found", null);

      if (vehicle.UserId != userId)
        return new Result<VehicleStatus>(false, "Forbidden", null);

      var newStatus = new VehicleStatus
      {
        VehicleId = request.VehicleId,
        Speed = request.Speed,
        FuelLevel = request.FuelLvl,
        EngineHealth = request.EngHlth,
        Timestamp = request.Date,
        Location = request.Location
      };

      context.VehicleStatuses.Add(newStatus);
      await context.SaveChangesAsync();

      await hubContext.Clients.All.SendAsync("VehicleStatusUpdated", newStatus);

      return new Result<VehicleStatus>(true, "Created", newStatus);
    }

    public async Task<Result<List<VehicleStatusDto>>> GetStatusesAsync(int userId)
    {
      var stauses = await context.VehicleStatuses
        .Where(vs => vs.Vehicle.UserId == userId)
        .Include(vs => vs.Vehicle)
        .OrderByDescending(vs => vs.Timestamp)
        .ToListAsync();

      var latestStatuses = stauses
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

      return new Result<List<VehicleStatusDto>>(true, "Success", latestStatuses);
    }

    public async Task<Result<List<VehicleDto>>> GetVehiclesAsync(int userId)
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

    public async Task<Result<List<VehicleStatusDto>>> GetVehicleHistoryAsync(int userId, int id)
    {
      var existingVehicle = await context.Vehicles
        .FirstOrDefaultAsync(v => v.UserId == userId && v.Id == id);

      if (existingVehicle == null)
        return new Result<List<VehicleStatusDto>>(false, "Vehicle does not exist", null);

      var statusHistory = await context.VehicleStatuses
        .Where(vs => vs.VehicleId == id && 
          vs.Vehicle.UserId == userId)
        .OrderByDescending(vs => vs.Timestamp)
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
        .ToListAsync();

      return new Result<List<VehicleStatusDto>>(true, "Success", statusHistory);
    }

    public async Task<Result> DeleteVehicleAsync(int userId, int id)
    {
      var existingVehicle = await context.Vehicles
        .FirstOrDefaultAsync(v => v.UserId == userId && v.Id == id);

      if (existingVehicle == null)
        return new Result(false, "Vehicle does not exist");

      context.Vehicles.Remove(existingVehicle);
      await context.SaveChangesAsync();

      logger.LogInformation(
        "Vehicle Deleted with ID: {vehicleId}",
        existingVehicle.Id);

      return new Result(true, "Deleted");
    }
  }
}