using JobMarket.Domain.Common;
using JobMarket.Domain.Enums;

namespace JobMarket.Domain.Entities;

public class SkillTrend : BaseEntity
{
    public string Skill { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public int DemandCount { get; private set; }
    public JobSource Source { get; private set; }

    private SkillTrend() { }

    public static SkillTrend Create(string skill, DateTime date, int demandCount, JobSource source)
        => new SkillTrend
        {
            Skill = skill,
            Date = date,
            DemandCount = demandCount,
            Source = source
        };
}
