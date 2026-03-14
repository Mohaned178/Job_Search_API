using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.SearchJobs;

public record SearchJobsQuery(JobSearchFilter Filter) : IRequest<PaginatedResult<JobSummaryDto>>;
