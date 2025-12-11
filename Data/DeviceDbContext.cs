using DeviceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceApi.Data
{
    public class DeviceDbContext : DbContext
    {
        public DeviceDbContext(DbContextOptions<DeviceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceLog> DeviceLogs { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>().ToTable("devices");
            modelBuilder.Entity<DeviceLog>().ToTable("device_logs");
            modelBuilder.Entity<AdminUser>().ToTable("admin_users");

            modelBuilder.Entity<AdminUser>().HasData(new AdminUser
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "39f8485ae66793496c7f4e437acfa60d3905653ea01ca155cf1b5d05446f3702",
                Role = "super_admin",
                CreatedAt = new DateTime(2025, 12, 5, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
