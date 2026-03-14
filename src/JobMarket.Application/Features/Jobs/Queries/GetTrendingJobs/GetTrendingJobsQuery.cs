using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.GetTrendingJobs;

public record GetTrendingJobsQuery(int Count = 20, string TimeWindow = "24h") : IRequest<List<JobSummaryDto>>;
