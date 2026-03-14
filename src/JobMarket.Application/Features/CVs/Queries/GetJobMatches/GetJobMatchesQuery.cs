using JobMarket.Application.Features.CVs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetJobMatches;

public record GetJobMatchesQuery(Guid CvId, Guid UserId, int TopK = 20) : IRequest<List<JobMatchDto>>;
