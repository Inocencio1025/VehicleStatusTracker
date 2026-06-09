using Microsoft.EntityFrameworkCore;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Data
{
    public class VehicleTrackerContext : DbContext
    {
        public VehicleTrackerContext(DbContextOptions<VehicleTrackerContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleStatus> VehicleStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleStatus>().OwnsOne(v => v.Location);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Vehicles)
                .WithOne(v => v.User)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.VehicleStatuses)
                .WithOne(vs => vs.Vehicle)
                .HasForeignKey(vs => vs.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.VIN)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
