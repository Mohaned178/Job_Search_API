using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Preferences.Queries.GetPersonalizedFeed;

public record GetPersonalizedFeedQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PaginatedResult<JobSummaryDto>>;
