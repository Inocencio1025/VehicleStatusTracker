namespace VehicleTrackerApi.Models
{
  public class Vehicle
  {
    public int Id { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string VIN { get; set; } = null!;
    public int UserId { get; set; } // foreign key
    public User User { get; set; } = null!;
    public List<VehicleStatus> VehicleStatuses { get; set; } = [];

    public Vehicle() { }

    public Vehicle(string make, string model, int year, string vin)
    {
      Make = make;
      Model = model;
      Year = year;
      VIN = vin;
    }
  }
}