namespace VehicleTrackerApi.Dtos
{
  public record VehicleDto(
      int VehicleId,
      string Make,
      string Model,
      int Year,
      string VIN
  );
}