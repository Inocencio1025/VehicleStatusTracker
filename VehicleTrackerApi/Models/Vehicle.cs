public class Vehicle {
    public int VehicleID { get; set; }
    public int Speed { get; set; }
    public Location Location { get; set; }
    public int FuelLvl { get; set; }
    public string EngineHealth { get; set; }
    public DateTime Date { get; set; }

    public Vehicle(int vehID, int speed, Location location, int fuelLvl, string engHlth, DateTime date) {
        VehicleID = vehID;
        Speed = speed;
        Location = location;
        FuelLvl = fuelLvl;
        EngineHealth = engHlth;
        Date = date;
    }
}