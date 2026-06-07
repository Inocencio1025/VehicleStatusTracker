namespace VehicleTrackerApi.Models
{
    public class VehicleStatus
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public int Speed { get; set; }
        public double FuelLevel { get; set; }
        public string EngineHealth { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Location Location { get; set; } = new();

        public VehicleStatus() { }

    }
}
