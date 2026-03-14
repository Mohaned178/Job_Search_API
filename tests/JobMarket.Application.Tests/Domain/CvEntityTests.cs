using FluentAssertions;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Enums;
using JobMarket.Domain.ValueObjects;

namespace JobMarket.Application.Tests.Domain;

public class CvEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsCv()
    {
        var cv = CV.Create(Guid.NewGuid(), "resume.pdf", "/storage/resume.pdf");

        cv.EnrichmentStatus.Should().Be(EnrichmentStatus.Pending);
        cv.OriginalFileName.Should().Be("resume.pdf");
    }

    [Fact]
    public void MarkProcessing_UpdatesStatus()
    {
        var cv = CV.Create(Guid.NewGuid(), "resume.pdf", "/storage/resume.pdf");

        cv.MarkProcessing();

        cv.EnrichmentStatus.Should().Be(EnrichmentStatus.Processing);
    }

    [Fact]
    public void SetParsedData_UpdatesDataAndStatus()
    {
        var cv = CV.Create(Guid.NewGuid(), "resume.pdf", "/storage/resume.pdf");
        var parsedData = new CvParsedData { FullName = "John Doe", Skills = new() { "C#" } };

        cv.SetParsedData(parsedData, "raw text content");

        cv.ParsedData.Should().NotBeNull();
        cv.RawText.Should().Be("raw text content");
        cv.EnrichmentStatus.Should().Be(EnrichmentStatus.Processing);
    }

    [Fact]
    public void MarkEnriched_SetsAllFieldsAndStatus()
    {
        var cv = CV.Create(Guid.NewGuid(), "resume.pdf", "/storage/resume.pdf");
        var parsedData = new CvParsedData { FullName = "Jane" };
        var embedding = new float[1536];
        var breakdown = new AtsScoreBreakdown { KeywordScore = 80, FormattingScore = 90, CompletenessScore = 85, SkillRelevanceScore = 75 };

        cv.MarkEnriched(parsedData, embedding, 82, breakdown);

        cv.AtsScore.Should().Be(82);
        cv.Embedding.Should().NotBeNull();
        cv.EnrichmentStatus.Should().Be(EnrichmentStatus.Enriched);
    }

    [Fact]
    public void MarkFailed_SetsFailedStatus()
    {
        var cv = CV.Create(Guid.NewGuid(), "resume.pdf", "/storage/resume.pdf");

        cv.MarkFailed();

        cv.EnrichmentStatus.Should().Be(EnrichmentStatus.Failed);
    }
}
