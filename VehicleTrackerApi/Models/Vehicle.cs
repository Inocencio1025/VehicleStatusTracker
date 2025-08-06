namespace VehicleTrackerApi.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int Speed { get; set; }
        public double FuelLevel { get; set; }
        public string EngineHealth { get; set; }
        public DateTime Timestamp { get; set; }
        public Location Location { get; set; } = new();

        public Vehicle() { }

        public Vehicle(int vehID, int speed, Location location, double fuelLvl, string engHlth, DateTime date)
        {
            VehicleId = vehID;
            Speed = speed;
            Location = location;
            FuelLevel = fuelLvl;
            EngineHealth = engHlth;
            Timestamp = date;
        }
    }
}
