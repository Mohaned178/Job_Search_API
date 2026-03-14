namespace JobMarket.Domain.ValueObjects;

public class Education
{
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Field { get; set; } = string.Empty;
}
