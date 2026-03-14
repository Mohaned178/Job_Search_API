using JobMarket.Application.Common.Interfaces;
using MediatR;

namespace JobMarket.Application.Features.Analytics.Queries.GetTrendingSkills;

public class GetTrendingSkillsHandler : IRequestHandler<GetTrendingSkillsQuery, List<TrendingSkillDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTrendingSkillsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TrendingSkillDto>> Handle(
        GetTrendingSkillsQuery request,
        CancellationToken cancellationToken)
    {
        var trends = await _unitOfWork.SkillTrends.GetAllAsync(cancellationToken);

        var trendingSkills = trends
            .GroupBy(t => t.Skill, StringComparer.OrdinalIgnoreCase)
            .Select(g => new TrendingSkillDto(
                g.Key,
                g.Sum(t => t.DemandCount),
                0))
            .OrderByDescending(s => s.DemandCount)
            .Take(request.Count)
            .ToList();

        return trendingSkills;
    }
}
