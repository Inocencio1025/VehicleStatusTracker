using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Dtos
{
  public record VehicleStatusDto(
      int VehicleId,
      string Make,
      string Model,
      int Year,
      int Speed,
      double FuelLevel,
      string EngineHealth,
      DateTime Timestamp,
      Location Location
  );
}