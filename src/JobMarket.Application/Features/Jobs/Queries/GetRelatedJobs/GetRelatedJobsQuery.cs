using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.GetRelatedJobs;

public record GetRelatedJobsQuery(Guid JobId, int TopK = 10) : IRequest<List<JobSummaryDto>>;
