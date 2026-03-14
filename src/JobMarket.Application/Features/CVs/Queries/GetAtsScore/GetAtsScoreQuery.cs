using JobMarket.Application.Features.CVs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetAtsScore;

public record GetAtsScoreQuery(Guid CvId, Guid UserId) : IRequest<AtsScoreResponse>;
