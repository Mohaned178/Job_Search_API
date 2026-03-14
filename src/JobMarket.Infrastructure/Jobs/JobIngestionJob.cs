using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Enums;
using JobMarket.Infrastructure.Persistence;
using JobMarket.Infrastructure.Scrapers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Jobs;

public class JobIngestionJob : IJobIngestionJob
{
    private readonly ScraperFactory _scraperFactory;
    private readonly AppDbContext _context;
    private readonly IJobEnqueuer _jobEnqueuer;
    private readonly ILogger<JobIngestionJob> _logger;

    public JobIngestionJob(
        ScraperFactory scraperFactory,
        AppDbContext context,
        IJobEnqueuer jobEnqueuer,
        ILogger<JobIngestionJob> logger)
    {
        _scraperFactory = scraperFactory;
        _context = context;
        _jobEnqueuer = jobEnqueuer;
        _logger = logger;
    }

    public async Task IngestAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting job ingestion");
        int totalNew = 0;

        var allStrategies = _scraperFactory.GetAll();
        var scraperOptions = new Application.Common.Interfaces.ScraperOptions { MaxPages = 5 };

        foreach (var strategy in allStrategies)
        {
            _logger.LogInformation("Scraping source {Source}", strategy.Source);

            try
            {
                var rawListings = await strategy.ScrapeAsync(scraperOptions, ct);

                foreach (var raw in rawListings)
                {
                    bool exists = await _context.Jobs.AnyAsync(
                        j => j.Source == strategy.Source && j.SourceJobId == raw.SourceJobId,
                        ct);

                    if (exists) continue;

                    Job job = Job.Create(
                        raw.Title,
                        raw.Company,
                        raw.Country,
                        raw.City,
                        raw.Description,
                        strategy.Source,
                        raw.SourceUrl,
                        raw.SourceJobId,
                        raw.PostedAt);

                    await _context.Jobs.AddAsync(job, ct);
                    totalNew++;
                }

                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Ingested {Count} new jobs from {Source}", totalNew, strategy.Source);
                totalNew = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest from source {Source}", strategy.Source);
            }
        }

        _jobEnqueuer.Enqueue<JobEnrichmentJob>(j => j.EnrichPendingAsync(CancellationToken.None));
    }
}
