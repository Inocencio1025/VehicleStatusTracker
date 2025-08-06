using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Data
{
    public class VehicleTrackerContext : DbContext
    {
        public VehicleTrackerContext(DbContextOptions<VehicleTrackerContext> options)
            : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>().OwnsOne(v => v.Location);

            base.OnModelCreating(modelBuilder);
        }
    }
}
