using JobMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobMarket.Infrastructure.Persistence.Configurations;

public class SkillTrendConfiguration : IEntityTypeConfiguration<SkillTrend>
{
    public void Configure(EntityTypeBuilder<SkillTrend> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Skill).HasMaxLength(200).IsRequired();

        builder.HasIndex(s => new { s.Skill, s.Date, s.Source });
    }
}
