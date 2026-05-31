namespace VehicleTrackerApi.Dtos
{
    public record RegisterRequest(
        string Username,
        string Email,
        string Password
    );
}