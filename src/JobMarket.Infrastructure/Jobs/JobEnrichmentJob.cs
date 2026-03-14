using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using JobMarket.Infrastructure.AI;
using JobMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Jobs;

public class JobEnrichmentJob
{
    private readonly AppDbContext _context;
    private readonly OpenAiJobEnrichmentService _enrichmentService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<JobEnrichmentJob> _logger;

    public JobEnrichmentJob(
        AppDbContext context,
        OpenAiJobEnrichmentService enrichmentService,
        IEmbeddingService embeddingService,
        ILogger<JobEnrichmentJob> logger)
    {
        _context = context;
        _enrichmentService = enrichmentService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task EnrichPendingAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting job enrichment for Pending jobs");

        List<Domain.Entities.Job> pendingJobs = await _context.Jobs
            .Where(j => j.EnrichmentStatus == EnrichmentStatus.Pending)
            .Take(50)
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} pending jobs to enrich", pendingJobs.Count);

        foreach (var job in pendingJobs)
        {
            try
            {
                job.MarkProcessing();
                await _context.SaveChangesAsync(ct);

                var enrichment = await _enrichmentService.EnrichAsync(job.Title, job.Description, ct);
                string embeddingText = $"{job.Title} {string.Join(" ", enrichment.Skills)}";
                float[] embedding = await _embeddingService.GenerateEmbeddingAsync(embeddingText, ct);

                job.Enrich(enrichment.Skills, enrichment.SeniorityLevel, enrichment.SalaryMin, enrichment.SalaryMax, enrichment.Currency, embedding);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Enriched job {JobId} with {SkillCount} skills", job.Id, enrichment.Skills.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enrich job {JobId}", job.Id);
                job.MarkFailed();
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
