using JobMarket.Application.Common.Models;

namespace JobMarket.Application.Common.Interfaces;

public interface IJobEnrichmentService
{
    Task<JobEnrichmentResult> EnrichAsync(string title, string description, CancellationToken ct = default);
}
