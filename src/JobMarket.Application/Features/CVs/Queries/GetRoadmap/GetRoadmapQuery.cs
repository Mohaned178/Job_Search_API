using JobMarket.Application.Features.CVs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetRoadmap;

public record GetRoadmapQuery(Guid CvId, Guid UserId, string TargetRole) : IRequest<RoadmapResponse>;
