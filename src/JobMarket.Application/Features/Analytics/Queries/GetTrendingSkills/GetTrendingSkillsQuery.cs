using MediatR;

namespace JobMarket.Application.Features.Analytics.Queries.GetTrendingSkills;

public record TrendingSkillDto(string Skill, int DemandCount, double ChangePercentage);

public record GetTrendingSkillsQuery(int Count = 20, string? Region = null) : IRequest<List<TrendingSkillDto>>;
