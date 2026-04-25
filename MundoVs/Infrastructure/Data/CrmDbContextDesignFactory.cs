using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MundoVs.Infrastructure.Data;

public class CrmDbContextDesignFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ZenithConnection")
            ?? config.GetConnectionString("ZenithConnection");

        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Connection string 'ZenithConnection' not configured. Use environment variables or local secrets.");

        var builder = new DbContextOptionsBuilder<CrmDbContext>();
        builder.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 11)));
        return new CrmDbContext(builder.Options);
    }
}
