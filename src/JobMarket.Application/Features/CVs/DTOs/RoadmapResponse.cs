namespace JobMarket.Application.Features.CVs.DTOs;

public class RoadmapResponse
{
    public Guid CvId { get; set; }
    public string TargetRole { get; set; } = string.Empty;
    public List<RoadmapStep> Steps { get; set; } = new();
}

public class RoadmapStep
{
    public int Order { get; set; }
    public string Skill { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int MarketDemandCount { get; set; }
    public string EstimatedTimeToLearn { get; set; } = string.Empty;
    public List<string> SuggestedResources { get; set; } = new();
}
