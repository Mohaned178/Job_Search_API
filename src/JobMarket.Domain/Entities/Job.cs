using JobMarket.Domain.Common;
using JobMarket.Domain.Enums;
using JobMarket.Domain.Exceptions;

namespace JobMarket.Domain.Entities;

public class Job : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Company { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public List<string> RequiredSkills { get; private set; } = new();
    public SeniorityLevel SeniorityLevel { get; private set; }
    public JobType JobType { get; private set; }
    public decimal? SalaryMin { get; private set; }
    public decimal? SalaryMax { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public JobSource Source { get; private set; }
    public string SourceUrl { get; private set; } = string.Empty;
    public string SourceJobId { get; private set; } = string.Empty;
    public float[]? Embedding { get; private set; }
    public EnrichmentStatus EnrichmentStatus { get; private set; }
    public DateTime PostedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private Job() { }

    public static Job Create(
        string title,
        string company,
        string country,
        string city,
        string description,
        JobSource source,
        string sourceUrl,
        string sourceJobId,
        DateTime postedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(sourceUrl))
            throw new DomainException("Source URL is required.");

        return new Job
        {
            Title = title,
            Company = company,
            Country = country,
            City = city,
            Description = description,
            Source = source,
            SourceUrl = sourceUrl,
            SourceJobId = sourceJobId,
            PostedAt = postedAt,
            EnrichmentStatus = EnrichmentStatus.Pending
        };
    }

    public void MarkProcessing()
    {
        EnrichmentStatus = EnrichmentStatus.Processing;
        SetUpdated();
    }

    public void Enrich(
        List<string> skills,
        SeniorityLevel level,
        decimal? salaryMin,
        decimal? salaryMax,
        string currency,
        float[]? embedding = null)
    {
        RequiredSkills = skills;
        SeniorityLevel = level;
        SalaryMin = salaryMin;
        SalaryMax = salaryMax;
        Currency = currency;
        if (embedding != null) Embedding = embedding;
        EnrichmentStatus = EnrichmentStatus.Enriched;
        SetUpdated();
    }

    public void SetEmbedding(float[] embedding)
    {
        Embedding = embedding;
        SetUpdated();
    }

    public void MarkFailed()
    {
        EnrichmentStatus = EnrichmentStatus.Failed;
        SetUpdated();
    }
}
