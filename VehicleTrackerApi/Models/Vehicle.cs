public class Vehicle
{
    public int VehicleId { get; set; }
    public int Speed { get; set; }
    public double FuelLevel { get; set; }
    public required Location Location { get; set; }
    public required string EngineHealth { get; set; }
    public DateTime Timestamp { get; set; }

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
