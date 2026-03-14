using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace JobMarket.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString =
            config.GetConnectionString("Postgres") ??
            "Host=localhost;Database=JobMarketDev;Username=postgres;Password=postgres";

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString, o => o.UseVector());

        return new AppDbContext(builder.Options);
    }
}
