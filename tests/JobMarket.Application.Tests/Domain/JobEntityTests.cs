using FluentAssertions;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Enums;
using JobMarket.Domain.Exceptions;

namespace JobMarket.Application.Tests.Domain;

public class JobEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsJob()
    {
        var job = Job.Create("Software Engineer", "Acme", "US", "NYC",
            "Great role", JobSource.Indeed, "https://example.com", "job-123", DateTime.UtcNow);

        job.Title.Should().Be("Software Engineer");
        job.EnrichmentStatus.Should().Be(EnrichmentStatus.Pending);
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsDomainException()
    {
        Action act = () => Job.Create("", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "https://example.com", "job-1", DateTime.UtcNow);

        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithEmptySourceUrl_ThrowsDomainException()
    {
        Action act = () => Job.Create("Engineer", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "", "job-1", DateTime.UtcNow);

        act.Should().Throw<DomainException>().WithMessage("*Source URL*");
    }

    [Fact]
    public void MarkProcessing_UpdatesStatus()
    {
        var job = Job.Create("Engineer", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "https://x.com", "j1", DateTime.UtcNow);

        job.MarkProcessing();

        job.EnrichmentStatus.Should().Be(EnrichmentStatus.Processing);
    }

    [Fact]
    public void Enrich_SetsSkillsAndStatus()
    {
        var job = Job.Create("Engineer", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "https://x.com", "j1", DateTime.UtcNow);
        var skills = new List<string> { "C#", "Docker" };

        job.Enrich(skills, SeniorityLevel.Mid, 50000, 80000, "USD");

        job.RequiredSkills.Should().BeEquivalentTo(skills);
        job.SeniorityLevel.Should().Be(SeniorityLevel.Mid);
        job.SalaryMin.Should().Be(50000);
        job.EnrichmentStatus.Should().Be(EnrichmentStatus.Enriched);
    }

    [Fact]
    public void Enrich_WithEmbedding_SetsEmbedding()
    {
        var job = Job.Create("Engineer", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "https://x.com", "j1", DateTime.UtcNow);
        var embedding = new float[1536];

        job.Enrich(new List<string>(), SeniorityLevel.Junior, null, null, "USD", embedding);

        job.Embedding.Should().NotBeNull();
        job.Embedding.Should().HaveCount(1536);
    }

    [Fact]
    public void MarkFailed_SetsFailedStatus()
    {
        var job = Job.Create("Engineer", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "https://x.com", "j1", DateTime.UtcNow);

        job.MarkFailed();

        job.EnrichmentStatus.Should().Be(EnrichmentStatus.Failed);
    }

    [Fact]
    public void SetEmbedding_SetsEmbeddingProperty()
    {
        var job = Job.Create("Engineer", "Acme", "US", "NYC", "desc",
            JobSource.Indeed, "https://x.com", "j1", DateTime.UtcNow);
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        job.SetEmbedding(embedding);

        job.Embedding.Should().BeEquivalentTo(embedding);
    }
}
