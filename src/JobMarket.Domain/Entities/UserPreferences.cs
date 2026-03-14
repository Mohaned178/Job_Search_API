using JobMarket.Domain.Common;
using JobMarket.Domain.Enums;

namespace JobMarket.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; private set; }
    public List<string> Categories { get; private set; } = new();
    public List<string> PreferredLocations { get; private set; } = new();
    public JobType? PreferredJobType { get; private set; }
    public SeniorityLevel? PreferredSeniority { get; private set; }

    private UserPreferences() { }

    public static UserPreferences Create(Guid userId)
        => new UserPreferences { UserId = userId };

    public void Update(
        List<string> categories,
        List<string> preferredLocations,
        JobType? preferredJobType,
        SeniorityLevel? preferredSeniority)
    {
        Categories = categories;
        PreferredLocations = preferredLocations;
        PreferredJobType = preferredJobType;
        PreferredSeniority = preferredSeniority;
    }
}
