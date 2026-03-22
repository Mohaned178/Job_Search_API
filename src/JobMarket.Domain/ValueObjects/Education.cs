namespace JobMarket.Domain.ValueObjects;

public class Education
{
    public string Degree { get; init; } = string.Empty;
    public string Institution { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Field { get; init; } = string.Empty;
}
