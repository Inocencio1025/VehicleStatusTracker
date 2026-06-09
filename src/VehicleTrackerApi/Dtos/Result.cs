namespace VehicleTrackerApi.Dtos
{
    public record Result(
        bool Success,
        string Message
    );

    // my first time implementing a generic, hopefully
    // this wont be stupid
    public record Result<T>(
        bool Success, 
        string Message, 
        T? Data);
}