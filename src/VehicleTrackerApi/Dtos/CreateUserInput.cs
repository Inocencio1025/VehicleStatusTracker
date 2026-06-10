namespace VehicleTrackerApi.Dtos
{
  public record CreateUserInput
  (
      string Username,
      string Email,
      string Password
  );
}
