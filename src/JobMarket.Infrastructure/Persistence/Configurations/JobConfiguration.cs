using System.Text.Json;
using JobMarket.Domain.Entities;
using JobMarket.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace JobMarket.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title).HasMaxLength(300).IsRequired();
        builder.Property(j => j.Company).HasMaxLength(200).IsRequired();
        builder.Property(j => j.SourceUrl).HasMaxLength(2000).IsRequired();
        builder.Property(j => j.SourceJobId).HasMaxLength(500).IsRequired();
        builder.Property(j => j.Currency).HasMaxLength(10);

        builder.Property(j => j.RequiredSkills)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        builder.HasIndex(j => new { j.Source, j.SourceJobId }).IsUnique();

        builder.Property(j => j.Embedding)
            .HasColumnType("vector(1536)")
            .HasConversion(
                v => v == null ? null : new Vector(v),
                v => v == null ? null : v.ToArray());

        builder.HasIndex(j => j.Embedding)
            .HasMethod("ivfflat")
            .HasOperators("vector_cosine_ops")
            .HasStorageParameter("lists", 100);
    }
}
