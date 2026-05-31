namespace VehicleTrackerApi.Dtos
{
    public record LoginRequest(
        string Username,
        string Password
    );
}