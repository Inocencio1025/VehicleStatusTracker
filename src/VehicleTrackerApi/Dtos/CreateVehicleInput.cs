namespace VehicleTrackerApi.Dtos
{
  public record CreateVehicleInput(
    string Make,
    string Model,
    int Year,
    string VIN
  );
}