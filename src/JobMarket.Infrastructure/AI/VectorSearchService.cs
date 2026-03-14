using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Common.Models;
using JobMarket.Application.Features.Jobs.DTOs;
using JobMarket.Domain.Entities;
using JobMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.AI;

public class VectorSearchService : IVectorSearchService
{
    private readonly AppDbContext _context;
    private readonly ILogger<VectorSearchService> _logger;

    public VectorSearchService(AppDbContext context, ILogger<VectorSearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<JobMatchResult>> FindSimilarJobsAsync(
        float[] queryVector,
        int topK,
        JobSearchFilter? filter = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Running vector similarity search with topK={TopK}", topK);

        string vectorString = $"[{string.Join(",", queryVector)}]";

        List<Job> jobs = await _context.Jobs
            .FromSqlRaw(
                "SELECT * FROM \"Jobs\" WHERE embedding IS NOT NULL ORDER BY embedding <=> {0}::vector LIMIT {1}",
                vectorString,
                topK * 3)
            .ToListAsync(ct);

        if (filter?.Country != null)
            jobs = jobs.Where(j => j.Country == filter.Country).ToList();
        if (filter?.SeniorityLevel != null)
            jobs = jobs.Where(j => j.SeniorityLevel == filter.SeniorityLevel).ToList();
        if (filter?.JobType != null)
            jobs = jobs.Where(j => j.JobType == filter.JobType).ToList();

        return jobs.Take(topK).Select((j, i) => new JobMatchResult
        {
            Job = j,
            MatchScore = 1.0 - (i * 0.01)
        }).ToList();
    }
}
