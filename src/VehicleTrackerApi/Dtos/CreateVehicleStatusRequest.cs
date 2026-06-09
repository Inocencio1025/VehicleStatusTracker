using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Dtos
{
  public record CreateVehicleStatusRequest(
    int VehicleId, 
    int Speed, 
    Location Location, 
    double FuelLvl, 
    string EngHlth, 
    DateTime Date
  );
}