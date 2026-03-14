using JobMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobMarket.API.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Applying pending database migrations");
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations");
            throw;
        }
    }
}
