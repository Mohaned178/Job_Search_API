namespace JobMarket.Application.Features.CVs.DTOs;

public class GapAnalysisResponse
{
    public Guid CvId { get; set; }
    public string TargetRole { get; set; } = string.Empty;
    public List<SkillGap> MissingSkills { get; set; } = new();
    public List<string> ExistingSkills { get; set; } = new();
}

public class SkillGap
{
    public string Skill { get; set; } = string.Empty;
    public int MarketDemandCount { get; set; }
    public string Priority { get; set; } = string.Empty;
}
