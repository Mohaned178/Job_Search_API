using JobMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace JobMarket.Infrastructure.Persistence.Configurations;

public class CvConfiguration : IEntityTypeConfiguration<CV>
{
    public void Configure(EntityTypeBuilder<CV> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.OriginalFileName).HasMaxLength(500);
        builder.Property(c => c.StoragePath).HasMaxLength(1000);

        builder.OwnsOne(c => c.ParsedData, pd =>
        {
            pd.ToJson();
            pd.OwnsMany(p => p.Experience);
            pd.OwnsMany(p => p.Education);
        });

        builder.OwnsOne(c => c.AtsBreakdown, ab => ab.ToJson());

        builder.Property(c => c.Embedding)
            .HasColumnType("vector(1536)")
            .HasConversion(
                v => v == null ? null : new Vector(v),
                v => v == null ? null : v.ToArray());

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
