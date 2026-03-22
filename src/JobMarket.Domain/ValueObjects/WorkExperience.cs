namespace JobMarket.Domain.ValueObjects;

public class WorkExperience
{
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Description { get; init; } = string.Empty;
}
