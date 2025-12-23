using DeviceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DeviceApi.Data
{
    public class ContractClientDbContext : DbContext
    {
        public ContractClientDbContext(DbContextOptions<ContractClientDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContractClient> ContractClient { get; set; }
        public DbSet<DeviceRouting> DeviceRoutings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContractClient>().ToTable("contract_client");
        }
    }
}