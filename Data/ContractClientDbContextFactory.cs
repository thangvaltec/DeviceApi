using DeviceApi.Data;
using Microsoft.EntityFrameworkCore;

public class ContractClientDbContextFactory
{
    public DeviceDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<DeviceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DeviceDbContext(options);
    }
}
