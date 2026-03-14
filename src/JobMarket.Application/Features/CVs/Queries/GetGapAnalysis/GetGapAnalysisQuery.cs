using JobMarket.Application.Features.CVs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetGapAnalysis;

public record GetGapAnalysisQuery(Guid CvId, Guid UserId, string TargetRole) : IRequest<GapAnalysisResponse>;
