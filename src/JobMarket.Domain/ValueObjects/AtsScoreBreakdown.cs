namespace JobMarket.Domain.ValueObjects;

public class AtsScoreBreakdown
{
    public int KeywordScore { get; set; }
    public int FormattingScore { get; set; }
    public int CompletenessScore { get; set; }
    public int SkillRelevanceScore { get; set; }
    public List<string> Suggestions { get; set; } = new();
}
