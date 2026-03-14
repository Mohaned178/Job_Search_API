using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetGapAnalysis;

public class GetGapAnalysisHandler : IRequestHandler<GetGapAnalysisQuery, GapAnalysisResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetGapAnalysisHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GapAnalysisResponse> Handle(GetGapAnalysisQuery request, CancellationToken cancellationToken)
    {
        var cv = await _unitOfWork.CVs.GetByIdAsync(request.CvId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CV), request.CvId);

        if (cv.UserId != request.UserId)
            throw new UnauthorizedException();

        if (cv.ParsedData == null)
            throw new DomainException("CV data is not yet available. CV is still being processed.");

        List<string> cvSkills = cv.ParsedData.Skills;

        var allTrends = await _unitOfWork.SkillTrends.GetAllAsync(cancellationToken);

        var missingSkillTrends = allTrends
            .Where(t => !cvSkills.Contains(t.Skill, StringComparer.OrdinalIgnoreCase))
            .GroupBy(t => t.Skill, StringComparer.OrdinalIgnoreCase)
            .Select(g => new SkillGap
            {
                Skill = g.Key,
                MarketDemandCount = g.Sum(t => t.DemandCount),
                Priority = g.Sum(t => t.DemandCount) > 100 ? "High" : g.Sum(t => t.DemandCount) > 50 ? "Medium" : "Low"
            })
            .OrderByDescending(s => s.MarketDemandCount)
            .ToList();

        return new GapAnalysisResponse
        {
            CvId = cv.Id,
            TargetRole = request.TargetRole,
            MissingSkills = missingSkillTrends,
            ExistingSkills = cvSkills
        };
    }
}
