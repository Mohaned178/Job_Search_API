using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetRoadmap;

public class GetRoadmapHandler : IRequestHandler<GetRoadmapQuery, RoadmapResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoadmapHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoadmapResponse> Handle(GetRoadmapQuery request, CancellationToken cancellationToken)
    {
        var cv = await _unitOfWork.CVs.GetByIdAsync(request.CvId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CV), request.CvId);

        if (cv.UserId != request.UserId)
            throw new UnauthorizedException();

        if (cv.ParsedData == null)
            throw new DomainException("CV data is not yet available. CV is still being processed.");

        List<string> cvSkills = cv.ParsedData.Skills;
        var allTrends = await _unitOfWork.SkillTrends.GetAllAsync(cancellationToken);

        var missingSkills = allTrends
            .Where(t => !cvSkills.Contains(t.Skill, StringComparer.OrdinalIgnoreCase))
            .GroupBy(t => t.Skill, StringComparer.OrdinalIgnoreCase)
            .Select(g => new { Skill = g.Key, DemandCount = g.Sum(t => t.DemandCount) })
            .OrderByDescending(s => s.DemandCount)
            .ToList();

        List<RoadmapStep> steps = missingSkills.Select((skill, index) => new RoadmapStep
        {
            Order = index + 1,
            Skill = skill.Skill,
            Reason = $"Required in {skill.DemandCount} job listings for {request.TargetRole} roles",
            MarketDemandCount = skill.DemandCount,
            EstimatedTimeToLearn = skill.DemandCount > 100 ? "2-4 weeks" : "1-2 weeks",
            SuggestedResources = new List<string>()
        }).ToList();

        return new RoadmapResponse
        {
            CvId = cv.Id,
            TargetRole = request.TargetRole,
            Steps = steps
        };
    }
}
