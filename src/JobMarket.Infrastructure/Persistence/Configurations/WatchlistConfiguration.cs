using JobMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobMarket.Infrastructure.Persistence.Configurations;

public class WatchlistConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasIndex(w => new { w.UserId, w.JobId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(w => w.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
