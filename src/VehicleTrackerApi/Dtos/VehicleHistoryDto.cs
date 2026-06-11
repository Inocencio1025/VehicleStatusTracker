namespace VehicleTrackerApi.Dtos
{
    public record VehicleHistoryDto(
        int AvgSpeed,
        int MaxSpeed,
        int TotalMileage,
        DateTime? LastRefueled,
        List<VehicleStatusDto> History
    );
}