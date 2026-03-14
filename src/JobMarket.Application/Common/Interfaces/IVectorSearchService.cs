using JobMarket.Application.Common.Models;
using JobMarket.Application.Features.Jobs.DTOs;

namespace JobMarket.Application.Common.Interfaces;

public interface IVectorSearchService
{
    Task<List<JobMatchResult>> FindSimilarJobsAsync(
        float[] queryVector,
        int topK,
        JobSearchFilter? filter = null,
        CancellationToken ct = default);
}
