namespace VehicleTrackerApi.Models
{
    public class User
    { 
        public int Id { get; set; }
        public  string Username { get; set; } = null!;
        public  string Email { get; set; } = null!;
        public  string Password { get; set; } = null!;
        public List<Vehicle> Vehicles { get; set; } = [];

        public User() { }
    }
}
