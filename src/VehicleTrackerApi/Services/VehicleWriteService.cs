using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Hubs;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
  public class VehicleWriteService(
    VehicleTrackerContext context,
    IHubContext<VehicleHub> hubContext,
    ILogger<VehicleWriteService> logger)
  {
    public async Task<Result<VehicleDto>> CreateVehicleAsync(
      int userId,
      CreateVehicleInput request)
    {
      if (string.IsNullOrWhiteSpace(request.Make) ||
          string.IsNullOrWhiteSpace(request.Model) ||
          string.IsNullOrWhiteSpace(request.VIN))
      {
        return new Result<VehicleDto>(false, "Missing required fields", null);
      }

      if (request.Year < 1000 || request.Year > 9999)
        return new Result<VehicleDto>(false, "Invalid year", null);

      var existingVehicle = await context.Vehicles.AnyAsync(v => v.VIN == request.VIN);

      if (existingVehicle)
        return new Result<VehicleDto>(false, "Vehicle already exists.", null);
      
      var newVehicle = new Vehicle(
          request.Make,
          request.Model,
          request.Year,
          request.VIN.Trim().ToUpper())
      {
        UserId = userId
      };

      var firstStatus = new VehicleStatus
      {
        Vehicle = newVehicle,
        Speed = 0,
        FuelLevel = 0,
        Timestamp = DateTime.UtcNow,
        Location = new Location(0, 0) 
      };

      context.Vehicles.Add(newVehicle);
      context.VehicleStatuses.Add(firstStatus);
      await context.SaveChangesAsync();

      logger.LogInformation(
        "Vehicle Created with ID: {VehicleId}",
        newVehicle.Id);

      return new Result<VehicleDto>(true, "Created", 
        new VehicleDto(
          newVehicle.Id,
          newVehicle.Make,
          newVehicle.Model,
          newVehicle.Year,
          newVehicle.VIN
        ));
    }

    public async Task<Result<VehicleStatusDto>> CreateVehicleStatusAsync(CreateVehicleStatusInput request)
    {
      var vehicle = await context.Vehicles
        .FirstOrDefaultAsync(v => v.Id == request.VehicleId);

      if (vehicle == null)
        return new Result<VehicleStatusDto>(false, "Vehicle not found", null);

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

      return new Result<VehicleStatusDto>(true, "Created", 
        new VehicleStatusDto(
          newStatus.VehicleId,
          vehicle.Make,
          vehicle.Model,
          vehicle.Year,
          newStatus.Speed,
          newStatus.FuelLevel,
          newStatus.EngineHealth,
          newStatus.Timestamp,
          newStatus.Location
        ));
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