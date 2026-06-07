namespace VehicleTrackerApi.Dtos
{
  public record CreateVehicleRequest(
    string Make,
    string Model,
    int Year,
    string VIN
  );
}