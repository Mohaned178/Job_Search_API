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

        List<string> conditions = ["embedding IS NOT NULL"];
        List<object> parameters = [vectorString, topK];

        if (filter?.Country is not null)
        {
            conditions.Add($"\"Country\" = {{{parameters.Count}}}");
            parameters.Add(filter.Country);
        }

        if (filter?.SeniorityLevel is not null)
        {
            conditions.Add($"\"SeniorityLevel\" = {{{parameters.Count}}}");
            parameters.Add((int)filter.SeniorityLevel.Value);
        }

        if (filter?.JobType is not null)
        {
            conditions.Add($"\"JobType\" = {{{parameters.Count}}}");
            parameters.Add((int)filter.JobType.Value);
        }

        string whereClause = string.Join(" AND ", conditions);

        List<Job> jobs = await _context.Jobs
            .FromSqlRaw(
                $"SELECT * FROM \"Jobs\" WHERE {whereClause} ORDER BY embedding <=> {{0}}::vector LIMIT {{1}}",
                parameters.ToArray())
            .ToListAsync(ct);

        return jobs.Select((j, i) => new JobMatchResult
        {
            Job = j,
            MatchScore = 1.0 - (i * 0.01)
        }).ToList();
    }
}
