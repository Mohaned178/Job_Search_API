namespace JobMarket.Domain.ValueObjects;

public class AtsScoreBreakdown
{
    public int KeywordScore { get; init; }
    public int FormattingScore { get; init; }
    public int CompletenessScore { get; init; }
    public int SkillRelevanceScore { get; init; }
    public List<string> Suggestions { get; init; } = new();
}
