using JobMarket.Application.Common.Models;
using JobMarket.Domain.Enums;

namespace JobMarket.Application.Common.Interfaces;

public interface IScraperStrategy
{
    JobSource Source { get; }
    Task<List<RawJobListing>> ScrapeAsync(ScraperOptions options, CancellationToken ct = default);
}

public record RawJobListing(
    string Title,
    string Company,
    string Country,
    string City,
    string Description,
    string SourceUrl,
    string SourceJobId,
    DateTime PostedAt,
    string? SalaryRaw);

public class ScraperOptions
{
    public int MaxPages { get; set; } = 5;
    public string? Keyword { get; set; }
    public string? Location { get; set; }
}
