using JobMarket.Domain.Common;
using JobMarket.Domain.Enums;
using JobMarket.Domain.ValueObjects;

namespace JobMarket.Domain.Entities;

public class CV : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string RawText { get; private set; } = string.Empty;
    public CvParsedData? ParsedData { get; private set; }
    public float[]? Embedding { get; private set; }
    public int? AtsScore { get; private set; }
    public AtsScoreBreakdown? AtsBreakdown { get; private set; }
    public EnrichmentStatus EnrichmentStatus { get; private set; }

    private CV() { }

    public static CV Create(Guid userId, string originalFileName, string storagePath)
    {
        return new CV
        {
            UserId = userId,
            OriginalFileName = originalFileName,
            StoragePath = storagePath,
            EnrichmentStatus = EnrichmentStatus.Pending
        };
    }

    public void SetParsedData(CvParsedData parsedData, string rawText)
    {
        ParsedData = parsedData;
        RawText = rawText;
        EnrichmentStatus = EnrichmentStatus.Processing;
        SetUpdated();
    }

    public void SetEmbedding(float[] embedding)
    {
        Embedding = embedding;
        SetUpdated();
    }

    public void MarkProcessing()
    {
        EnrichmentStatus = EnrichmentStatus.Processing;
        SetUpdated();
    }

    public void MarkEnriched(
        CvParsedData parsedData,
        float[] embedding,
        int atsScore,
        AtsScoreBreakdown atsBreakdown)
    {
        ParsedData = parsedData;
        Embedding = embedding;
        AtsScore = atsScore;
        AtsBreakdown = atsBreakdown;
        EnrichmentStatus = EnrichmentStatus.Enriched;
        SetUpdated();
    }

    public void SetAtsScore(int score, AtsScoreBreakdown breakdown)
    {
        AtsScore = score;
        AtsBreakdown = breakdown;
        EnrichmentStatus = EnrichmentStatus.Enriched;
        SetUpdated();
    }

    public void MarkFailed()
    {
        EnrichmentStatus = EnrichmentStatus.Failed;
        SetUpdated();
    }
}
